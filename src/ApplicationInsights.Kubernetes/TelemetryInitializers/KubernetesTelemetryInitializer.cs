using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using static Microsoft.ApplicationInsights.Kubernetes.TelemetryProperty;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    /// <summary>
    /// Telemetry Initializer for K8s Environment
    /// </summary>
    internal class KubernetesTelemetryInitializer : ITelemetryInitializer
    {
        private readonly SDKVersionUtils _sdkVersionUtils;
        private readonly DateTime _timeoutAt;

        internal readonly IK8sEnvironmentFactory _k8sEnvFactory;
        internal IK8sEnvironment _k8sEnvironment { get; private set; }

        internal AppInsightsForKubernetesOptions _options { get; private set; }

        internal bool _isK8sQueryTimeout = false;
        private bool _isK8sQueryTimeoutReported = false;
        private static readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
        internal readonly ITelemetryKeyCache _telemetryKeyCache;

        public KubernetesTelemetryInitializer(
            IK8sEnvironmentFactory k8sEnvFactory,
            IOptions<AppInsightsForKubernetesOptions> options,
            SDKVersionUtils sdkVersionUtils,
            ITelemetryKeyCache telemetryKeyCache)
        {
            _k8sEnvironment = null;

            // Options can't be null.
            Debug.Assert(options != null, "Options can't be null.");
            _options = Arguments.IsNotNull(options?.Value, nameof(options));

            _logger.LogDebug(@"Initialize Application Insights for Kubernetes telemetry initializer with Options:
{0}", JsonConvert.SerializeObject(_options));

            _telemetryKeyCache = telemetryKeyCache ?? throw new ArgumentNullException(nameof(telemetryKeyCache));
            _sdkVersionUtils = Arguments.IsNotNull(sdkVersionUtils, nameof(sdkVersionUtils));
            _timeoutAt = DateTime.Now.Add(_options.InitializationTimeout);
            _k8sEnvFactory = Arguments.IsNotNull(k8sEnvFactory, nameof(k8sEnvFactory));

            _ = SetK8sEnvironment();
        }

        public void Initialize(ITelemetry telemetry)
        {
            IK8sEnvironment k8sEnv = _k8sEnvironment;
            if (k8sEnv != null)
            {
                _logger.LogTrace("Application Insights for Kubernetes telemetry initializer is invoked.", k8sEnv.PodName);
                try
                {
                    InitializeTelemetry(telemetry, k8sEnv);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
            else
            {
                _logger.LogTrace("Application Insights for Kubernetes telemetry initializer is used but the content has not ready yet.");

                if (_isK8sQueryTimeout)
                {
                    if (!_isK8sQueryTimeoutReported)
                    {
                        _isK8sQueryTimeoutReported = true;
                        _logger.LogError("Query Kubernetes Environment timeout.");
                    }
                }
            }

            telemetry.Context.GetInternalContext().SdkVersion = _sdkVersionUtils.CurrentSDKVersion;
        }

        private void InitializeTelemetry(ITelemetry telemetry, IK8sEnvironment k8sEnv)
        {
            // Setting the container name to role name
            if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
            {
                telemetry.Context.Cloud.RoleName = k8sEnv.ContainerName;
            }

            if (telemetry is ISupportProperties propertySetter)
            {
                SetCustomDimensions(propertySetter);
            }
            else
            {
                _logger.LogTrace("This telemetry object doesn't implement ISupportProperties.");
            }
            _logger.LogTrace("Finish telemetry initializer.");
        }

        private async Task SetK8sEnvironment()
        {
            Task<IK8sEnvironment> createK8sEnvTask = _k8sEnvFactory.CreateAsync(_timeoutAt);
            await Task.WhenAny(
                createK8sEnvTask,
                Task.Delay(_timeoutAt - DateTime.Now)).ConfigureAwait(false);

            if (createK8sEnvTask.IsCompleted)
            {
                _logger.LogDebug("Application Insights for Kubernetes environment initialized.");
                _k8sEnvironment = createK8sEnvTask.Result;
            }
            else
            {
                _isK8sQueryTimeout = true;
                _k8sEnvironment = null;
                _logger.LogError("Application Insights for Kubernetes environment initialization timed out.");
            }
        }

        private void SetCustomDimensions(ISupportProperties telemetry)
        {
            // Container
            SetCustomDimension(telemetry, Container.ID, this._k8sEnvironment.ContainerID);
            SetCustomDimension(telemetry, Container.Name, this._k8sEnvironment.ContainerName);

            // Pod
            SetCustomDimension(telemetry, Pod.ID, this._k8sEnvironment.PodID);
            SetCustomDimension(telemetry, Pod.Name, this._k8sEnvironment.PodName);
            SetCustomDimension(telemetry, Pod.Labels, this._k8sEnvironment.PodLabels);
            SetCustomDimension(telemetry, Pod.Namespace, this._k8sEnvironment.PodNamespace, isValueOptional: true);

            // Pod will have no replica name or deployment when deployed through other means. For example, as a daemonset.
            // Replica Set
            SetCustomDimension(telemetry, ReplicaSet.Name, this._k8sEnvironment.ReplicaSetName, isValueOptional: true);

            // Deployment
            SetCustomDimension(telemetry, Deployment.Name, this._k8sEnvironment.DeploymentName, isValueOptional: true);

            // Node
            SetCustomDimension(telemetry, Node.ID, this._k8sEnvironment.NodeUid);
            SetCustomDimension(telemetry, Node.Name, this._k8sEnvironment.NodeName);
        }

        private void SetCustomDimension(ISupportProperties telemetry, string key, string value, bool isValueOptional = false)
        {
            key = _telemetryKeyCache.GetTelemetryProcessedKey(key);

            if (telemetry == null)
            {
                _logger.LogError("telemetry object is null in telemetry initializer.");
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                _logger.LogError("Key is required to set custom dimension.");
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                if (isValueOptional)
                {
                    _logger.LogTrace("Value is null or empty for key: {0}", key);
                }
                else
                {
                    _logger.LogError("Value is required to set custom dimension. Key: {0}", key);
                }
                return;
            }

            if (!telemetry.Properties.ContainsKey(key))
            {
                telemetry.Properties.Add(key, value);
            }
            else
            {
                string existingValue = telemetry.Properties[key];
                if (string.Equals(existingValue, value, System.StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogTrace("The telemetry already contains the property of {0} with the same value of {1}.", key, existingValue);
                }
                else
                {
                    telemetry.Properties[key] = value;
                    _logger.LogDebug("The telemetry already contains the property of {0} with value {1}. The new value is: {2}", key, existingValue, value);
                }
            }
        }
    }
}

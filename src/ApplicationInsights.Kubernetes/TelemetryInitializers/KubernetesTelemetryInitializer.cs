using System;
using System.Diagnostics;
using System.Threading;
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
        private static readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

        private readonly SDKVersionUtils _sdkVersionUtils;
        private readonly DateTime _timeoutAt;
        private bool _isK8sQueryTimeoutReported = false;

        internal AppInsightsForKubernetesOptions Options { get; }
        internal ITelemetryKeyCache TelemetryKeyCache { get; }
        internal IK8sEnvironmentFactory K8sEnvFactory { get; }
        internal bool IsK8sQueryTimeout { get; private set; } = false;
        internal IK8sEnvironment? K8sEnvironment { get; private set; }

        public KubernetesTelemetryInitializer(
            IK8sEnvironmentFactory k8sEnvFactory,
            IOptions<AppInsightsForKubernetesOptions> options,
            SDKVersionUtils sdkVersionUtils,
            ITelemetryKeyCache telemetryKeyCache)
        {
            K8sEnvironment = null;

            // Options can't be null.
            Debug.Assert(options != null, "Options can't be null.");
            Options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            _logger.LogDebug(@"Initialize Application Insights for Kubernetes telemetry initializer with Options:
{0}", JsonConvert.SerializeObject(Options));

            TelemetryKeyCache = telemetryKeyCache ?? throw new ArgumentNullException(nameof(telemetryKeyCache));
            _sdkVersionUtils = sdkVersionUtils ?? throw new ArgumentNullException(nameof(sdkVersionUtils));
            _timeoutAt = DateTime.Now.Add(Options.InitializationTimeout);
            K8sEnvFactory = k8sEnvFactory ?? throw new ArgumentNullException(nameof(k8sEnvFactory));

            _ = SetK8sEnvironment(default);
        }

        public void Initialize(ITelemetry telemetry)
        {
            IK8sEnvironment? k8sEnv = K8sEnvironment;
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

                if (IsK8sQueryTimeout)
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
                SetCustomDimensions(propertySetter, k8sEnv);
            }
            else
            {
                _logger.LogTrace("This telemetry object doesn't implement ISupportProperties.");
            }
            _logger.LogTrace("Finish telemetry initializer.");
        }

        private async Task SetK8sEnvironment(CancellationToken cancellationToken)
        {
            try
            {
                Task<IK8sEnvironment?> createK8sEnvTask = K8sEnvFactory.CreateAsync(_timeoutAt, cancellationToken);
                await Task.WhenAny(
                    createK8sEnvTask,
                    Task.Delay(_timeoutAt - DateTime.Now)).ConfigureAwait(false);

                if (createK8sEnvTask.IsCompleted && createK8sEnvTask.Result is not null)
                {
                    _logger.LogDebug("Application Insights for Kubernetes environment initialized.");
                    K8sEnvironment = createK8sEnvTask.Result;
                }
                else
                {
                    IsK8sQueryTimeout = true;
                    K8sEnvironment = null;
                    _logger.LogError("Application Insights for Kubernetes environment initialization failed. Please review the logs for details.");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                // In case of any unexpected exception, we shall log it than let it throw.
                _logger.LogError("Unexpected error happened. Telemetry enhancement with Kubernetes info won't happen. Message: {0}", ex.Message);
                _logger.LogTrace("Unexpected error happened. Telemetry enhancement with Kubernetes info won't happen. Details: {0}", ex.ToString());
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private void SetCustomDimensions(ISupportProperties telemetry, IK8sEnvironment k8sEnvironment)
        {
            // Container 
            SetCustomDimension(telemetry, Container.ID, k8sEnvironment.ContainerID, isValueOptional: true);
            SetCustomDimension(telemetry, Container.Name, k8sEnvironment.ContainerName, isValueOptional: true);

            // Pod
            SetCustomDimension(telemetry, Pod.ID, k8sEnvironment.PodID);
            SetCustomDimension(telemetry, Pod.Name, k8sEnvironment.PodName);
            SetCustomDimension(telemetry, Pod.Labels, k8sEnvironment.PodLabels, isValueOptional: true);
            SetCustomDimension(telemetry, Pod.Namespace, k8sEnvironment.PodNamespace, isValueOptional: true);

            // Pod will have no replica name or deployment when deployed through other means. For example, as a daemonset.
            // Replica Set
            SetCustomDimension(telemetry, ReplicaSet.Name, k8sEnvironment.ReplicaSetName, isValueOptional: true);

            // Deployment
            SetCustomDimension(telemetry, Deployment.Name, k8sEnvironment.DeploymentName, isValueOptional: true);

            // Node
            SetCustomDimension(telemetry, Node.ID, k8sEnvironment.NodeUid, isValueOptional: true);
            SetCustomDimension(telemetry, Node.Name, k8sEnvironment.NodeName, isValueOptional: true);
        }

        private void SetCustomDimension(ISupportProperties telemetry, string key, string? value, bool isValueOptional = false)
        {
            key = TelemetryKeyCache.GetProcessedKey(key);

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

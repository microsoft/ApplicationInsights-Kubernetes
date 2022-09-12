using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using static Microsoft.ApplicationInsights.Kubernetes.TelemetryProperty;

namespace Microsoft.ApplicationInsights.Kubernetes;

/// <summary>
/// Telemetry Initializer for K8s Environment
/// </summary>
internal class KubernetesTelemetryInitializer : ITelemetryInitializer
{
    private static readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
    private readonly SDKVersionUtils _sdkVersionUtils;
    internal IK8sEnvironmentHolder K8SEnvironmentHolder { get; }
    internal ITelemetryKeyCache TelemetryKeyCache { get; }

    public KubernetesTelemetryInitializer(
        IK8sEnvironmentHolder k8sEnvironmentHolder,
        SDKVersionUtils sdkVersionUtils,
        ITelemetryKeyCache telemetryKeyCache)
    {
        TelemetryKeyCache = telemetryKeyCache ?? throw new ArgumentNullException(nameof(telemetryKeyCache));
        K8SEnvironmentHolder = k8sEnvironmentHolder ?? throw new ArgumentNullException(nameof(k8sEnvironmentHolder));
        _sdkVersionUtils = sdkVersionUtils ?? throw new ArgumentNullException(nameof(sdkVersionUtils));
    }

    public void Initialize(ITelemetry telemetry)
    {
        IK8sEnvironment? k8sEnv = K8SEnvironmentHolder.K8sEnvironment;

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

        if (telemetry is null)
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

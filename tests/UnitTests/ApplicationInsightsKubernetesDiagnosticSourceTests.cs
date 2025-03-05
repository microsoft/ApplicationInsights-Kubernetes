using System;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

public class ApplicationInsightsKubernetesDiagnosticSourceTests
{
    [Fact]
    public void ShouldHandleKubernetesErrors()
    {
        var target = ApplicationInsightsKubernetesDiagnosticSource.Instance;

        // sample AKS exception message with {}
        var ex = new k8s.Autorest.HttpOperationException("Operation returned an invalid status code 'Forbidden', response body {\"kind\":\"Status\",\"apiVersion\":\"v1\",\"metadata\":{},\"status\":\"Failure\",\"message\":\"nodes is forbidden: User \"system:serviceaccount:xxx\" cannot list resource \"nodes\" in API group \"\" at the cluster scope\",\"reason\":\"Forbidden\",\"details\":{\"kind\":\"nodes\"},\"code\":403}");

        target.LogDebug(ex.Message);
        target.LogTrace(ex.ToString());
    }
}

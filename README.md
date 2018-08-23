Microsoft Application Insights for Kubernetes
==
This repository has code for Application Insights for Kubernetes, which works on .NET Core applications within the containers, managed by Kubernetes, on Azure Container Service.

**Note:** `Microsoft Application Insights for Kubernetes` (this library) is an enhancement to the [Microsoft Application Insights](https://github.com/Microsoft/ApplicationInsights-aspnetcore). You can choose to run **Application Insights** without this library in Kubernetes cluster too. However, when using `Microsoft Application Insights for Kubernetes`, you will see Kubernetes related properties like *Pod-Name, Deployment ...* on all your telemetry entries. Proper values will also be set to make use of the rich features like enabling the Application Map to show the multiple micro services on the same map.

### Continous Integration Status
|Rolling Build                    | Nightly Build                |
|---------------------------------|:-----------------------------|
|![Rolling-Build Status](https://devdiv.visualstudio.com/_apis/public/build/definitions/0bdbc590-a062-4c3f-b0f6-9383f67865ee/5974/badge) | ![Nightly-Build Status](https://devdiv.visualstudio.com/_apis/public/build/definitions/0bdbc590-a062-4c3f-b0f6-9383f67865ee/5976/badge) |

## Get Started
### Prerequisite
* [Application Insights for ASP.NET Core](https://github.com/Microsoft/ApplicationInsights-aspnetcore)
* [Docker Containers](https://www.docker.com/)
* [Kubernetes](https://kubernetes.io/)

### Walkthrough
We support **ASP.NET Core** application as well as **.NET Core** application.

* For **ASP.NET Core** Application: Refer [Getting Started](https://github.com/Microsoft/ApplicationInsights-Kubernetes/wiki/Getting-Started-for-ASP.NET-Core-Applications) for a simple walkthrough.

* For **.NET Core** Application: Refer [Getting Started](https://github.com/Microsoft/ApplicationInsights-Kubernetes/wiki/Getting-Started-for-.NET-Core-Applications) for a simple walkthrough.

* Follow [this example](examples/BasicUsage_clr21_RBAC) for Role-based access control (RBAC) enabled Kubernetes clusters.

### Verify the cluster configuration
Use the [troubleshooting image](https://github.com/Microsoft/ApplicationInsights-Kubernetes/tree/develop/troubleshooting) to verify the cluster is properly configured.

### Learn more
* To build a container for Kubernetes that have Application Insights baked in for the existing applications, please refer the example of [Zero Code light up](https://github.com/Microsoft/ApplicationInsights-Kubernetes/tree/develop/examples/ZeroUserCodeLightup).
* To enable Application Insights for Kubernetes by environement variable instead of code, please refer [Hosting startup for ApplicationInsights.Kubernetes](https://github.com/Microsoft/ApplicationInsights-Kubernetes/wiki/Hosting-startup-for-ApplicationInsights.Kubernetes).
* Still want more? Read the [Wikis](https://github.com/Microsoft/ApplicationInsights-Kubernetes/wiki).

### Next step
Profile your application for performance improvement using [Application Insights Profiler for Linux](https://github.com/Microsoft/ApplicationInsights-Profiler-AspNetCore).

## Contributing
### Report issues
Please file bug, discussion or any other interesting topics in [issues](https://github.com/Microsoft/ApplicationInsights-Kubernetes/issues) on github.

### Trouble Shooting
When Microsoft.ApplicationInsights.Kubernetes doesn't work properly, you can turn on self-diagnostics to see the traces in Kubernetes' logs. Refer [this wiki page](https://github.com/Microsoft/ApplicationInsights-Kubernetes/wiki/%5BAdvanced%5D-How-to-enable-self-diagnostics-for-ApplicationInsights.Kubernetes) for instructions to turn on trace.

### Developing
Please refer the [Develop Guide](https://github.com/Microsoft/ApplicationInsights-Kubernetes/wiki/Development-Guide).


---
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

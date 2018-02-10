Microsoft Application Insights for Kubernetes
==
This repository has code for Application Insights for Kubernetes, which works on .NET Core applications within the containers, managed by Kubernetes, on Azure Container Service.
This page is subject to be updated once more info is available.

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
For ASP.NET Core Application: Refer [Getting Started](https://github.com/Microsoft/ApplicationInsights-Kubernetes/wiki/Getting-Started-for-ASP.NET-Core-Applications) for a simple walkthrough.

For .NET Core Application: Refer [Getting Started](https://github.com/Microsoft/ApplicationInsights-Kubernetes/wiki/Getting-Started-for-.NET-Core-Applications) for a simple walkthrough.

To enable Application Insights for Kubernetes by environement variable instead of code, please refer [Hosting startup for ApplicationInsights.Kubernetes](https://github.com/Microsoft/ApplicationInsights-Kubernetes/wiki/Hosting-startup-for-ApplicationInsights.Kubernetes).

## Contributing
### Report issues
Please file bug, discussion or any other interesting topics in [issues](https://github.com/Microsoft/ApplicationInsights-Kubernetes/issues) on github.

### Trouble Shooting
When Microsoft.ApplicationInsights.Kubernetes doesn't work properly, you can turn on self-diagnostics to see the traces in Kubernetes' logs. Refer [this wiki page](https://github.com/Microsoft/ApplicationInsights-Kubernetes/wiki/%5BAdvanced%5D-How-to-enable-self-diagnostics-for-ApplicationInsights.Kubernetes) for instructions to turn on trace.

### Developing
Please refer the [Develop Guide](https://github.com/Microsoft/ApplicationInsights-Kubernetes/wiki/Development-Guide).


---
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

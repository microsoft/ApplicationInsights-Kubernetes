Microsoft Application Insights for Kubernetes
==
This repository has code for Application Insights for Kubernetes, which works on .NET Core applications within the containers, managed by Kubernetes, on Azure Container Service.

# Continous Integration Status
|Rolling Build                    | Nightly Build                |
|---------------------------------|:-----------------------------|
|![Rolling-Build Status](https://devdiv.visualstudio.com/_apis/public/build/definitions/0bdbc590-a062-4c3f-b0f6-9383f67865ee/5974/badge) | ![Nightly-Build Status](https://devdiv.visualstudio.com/_apis/public/build/definitions/0bdbc590-a062-4c3f-b0f6-9383f67865ee/5976/badge) |

# Get Started
## Prerequisite
* [Application Insights for ASP.NET Core](https://github.com/Microsoft/ApplicationInsights-aspnetcore)
* [Docker Containers](https://www.docker.com/)
* [Kubernetes](https://kubernetes.io/)

## Walkthrough
### Build the application and enable application insights
### Create the containers
### Create the cluster
### Deploy the containers onto the cluster

# Contributing
## Report issues
Please file bug, discussion or any other interesting topics in [issues](https://github.com/Microsoft/ApplicationInsights-Kubernetes/issues) on github.

## Developing
### Prerequisite
* [Visual Studio 2017](https://www.visualstudio.com/downloads/)

### Build
Firstly, please check your .NET Core CLI version by running:

    dotnet --version

And the expected version is: 1.0.1

Then, restore the nuget packages when necessary:

    dotnet restore src\aikubequery.sln

Build the product:

    dotnet build src\aikubequery.sln

### Test
We uses [xUnit](https://xunit.github.io/) and [Moq](https://github.com/Moq/moq4/wiki/Quickstart) for our unit tests.

### Code-Conventions
We uses C# and please follow the coding conventions here: [C# Coding Conventions](https://msdn.microsoft.com/en-us/library/ff926074.aspx). When there is special conventions, we will post it in Wiki.

### Branches
* The default branch is 'develop'. Please submit all pull requests for new features to this branch.
* For new features, create branch like feature/<feature-name>, for example:

    
        feature/better-readme

* For bug fixes, create branch like bug/<issue-number>, for exmaple, if the issue to track the bug is #100:
        
        bug/100

* Once the features are integrated and ready to release, it will be merged back to master. All releases will be shipped out of master branch. The specific commit will be tagged for releases.

---
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

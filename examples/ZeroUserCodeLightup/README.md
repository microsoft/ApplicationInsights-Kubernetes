# Zero Code light up Example
This walkthrough shows how to enable application insights for Kubernetes for ASP.NET Core 2.0 Web App without code change for the existing project.

All you need to do is to add 3 lines in the [Dockerfile](./Dockerfile), and they are:

```dockerfile
...
# Adding a reference to hosting startup package
RUN dotnet add package Microsoft.ApplicationInsights.Kubernetes.HostingStartup

# Light up Application Insights for Kubernetes
ENV APPINSIGHTS_INSTRUMENTATIONKEY Your_ApplicaitonInsights_Instrumentation_Key
ENV ASPNETCORE_HOSTINGSTARTUPASSEMBLIES Microsoft.ApplicationInsights.Kubernetes.HostingStartup
...
```
Full file for your reference:

```dockerfile
FROM microsoft/aspnetcore-build:2.0.5-2.1.4 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./

# Adding reference to hosting startup package
RUN dotnet add package Microsoft.ApplicationInsights.Kubernetes.HostingStartup
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/aspnetcore:2.0.5

# Lightup Application Insights for Kubernetes
ENV APPINSIGHTS_INSTRUMENTATIONKEY Your_ApplicaitonInsights_Instrumentation_Key
ENV ASPNETCORE_HOSTINGSTARTUPASSEMBLIES Microsoft.ApplicationInsights.Kubernetes.HostingStartup

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "ZeroUserCodeLightup.dll"]
```

**Note:** This applies to 1.0.6-beta1 and above.
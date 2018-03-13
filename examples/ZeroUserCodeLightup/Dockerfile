FROM microsoft/aspnetcore-build:2.0.5-2.1.4 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./

# Adding a reference to hosting startup package
RUN dotnet add package Microsoft.ApplicationInsights.Kubernetes.HostingStartup
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/aspnetcore:2.0.5

# Light up Application Insights for Kubernetes
ENV APPINSIGHTS_INSTRUMENTATIONKEY Your_ApplicaitonInsights_Instrumentation_Key
ENV ASPNETCORE_HOSTINGSTARTUPASSEMBLIES Microsoft.ApplicationInsights.Kubernetes.HostingStartup

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "ZeroUserCodeLightup.dll"]
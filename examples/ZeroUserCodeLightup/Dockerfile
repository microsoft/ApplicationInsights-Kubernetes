FROM microsoft/aspnetcore-build:2.0.5-2.1.4 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./

# Adding a reference to hosting startup package
RUN dotnet add package Microsoft.ApplicationInsights.Kubernetes.HostingStartup -v 1.0.0-*

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/aspnetcore:2.0.5

# Create an argument to allow docker builder to passing in application insights key.
# For example: docker build . --build-arg APPINSIGHTS_KEY=YOUR_APPLICATIONINSIGHTS_INSTRUMENTATION_KEY
ARG APPINSIGHTS_KEY
# Making sure the argument is set. Fail the build of the container otherwise.
RUN test -n "$APPINSIGHTS_KEY"

# Light up Application Insights for Kubernetes
ENV APPINSIGHTS_INSTRUMENTATIONKEY $APPINSIGHTS_KEY
ENV ASPNETCORE_HOSTINGSTARTUPASSEMBLIES Microsoft.ApplicationInsights.Kubernetes.HostingStartup

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "ZeroUserCodeLightup.dll"]
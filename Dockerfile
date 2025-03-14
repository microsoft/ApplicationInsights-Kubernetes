FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY troubleshooting/AIK8sTroubleShooting/*.csproj /app/troubleshooting/AIK8sTroubleShooting/
COPY src/ApplicationInsights.Kubernetes/*.csproj /app/src/ApplicationInsights.Kubernetes/

WORKDIR /app/troubleshooting/AIK8sTroubleShooting
RUN dotnet restore

# Copy everything else and build
COPY troubleshooting/AIK8sTroubleShooting /app/troubleshooting/AIK8sTroubleShooting
COPY src /app/src
COPY LICENSE /app/LICENSE
COPY README.md /app/README.md
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build-env /app/troubleshooting/AIK8sTroubleShooting/out .
ENTRYPOINT ["dotnet", "AIK8sTroubleShooting.dll"]
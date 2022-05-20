FROM mcr.microsoft.com/dotnet/aspnet:3.1-nanoserver-ltsc2022 AS base
WORKDIR /app
EXPOSE 80 443

FROM mcr.microsoft.com/dotnet/sdk:3.1-nanoserver-ltsc2022 AS build
WORKDIR /src
COPY AspNetCoreNano.csproj .
COPY NuGet.config .
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app

COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AspNetCoreNano.dll"]

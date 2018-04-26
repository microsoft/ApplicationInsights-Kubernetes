FROM microsoft/aspnetcore:2.0.3-nanoserver-1709 AS base
WORKDIR /app
EXPOSE 80 443

FROM microsoft/aspnetcore-build:2.0.3-nanoserver-1709 AS build
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

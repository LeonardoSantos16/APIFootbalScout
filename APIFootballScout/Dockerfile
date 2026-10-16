# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
# aspnet 10.0.11 fixado por digest (manifest list: amd64, arm64, arm/v7)
FROM mcr.microsoft.com/dotnet/aspnet:10.0.11@sha256:011bb5f30180717b1c8b65822ff2c99bcb96bc65af0164589751b83c7b4949f7 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
# sdk 10.0.400 (runtime 10.0.11) fixado por digest
FROM mcr.microsoft.com/dotnet/sdk:10.0.400@sha256:4beef5b8919dcaa2dc924233bd069257e883cc7a061e09088a97d152d6a48510 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["global.json", "Directory.Build.props", "./"]
COPY ["APIFootballScout/APIFootballScout.csproj", "APIFootballScout/packages.lock.json", "APIFootballScout/"]
COPY ["APIFootballScout.ServiceDefaults/APIFootballScout.ServiceDefaults.csproj", "APIFootballScout.ServiceDefaults/packages.lock.json", "APIFootballScout.ServiceDefaults/"]
RUN dotnet restore "./APIFootballScout/APIFootballScout.csproj" --locked-mode
COPY . .
WORKDIR "/src/APIFootballScout"
RUN dotnet build "./APIFootballScout.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./APIFootballScout.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "APIFootballScout.dll"]
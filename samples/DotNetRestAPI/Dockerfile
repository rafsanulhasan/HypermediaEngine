# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/HypermediaEngine/Directory.Build.props", "src/HypermediaEngine/"]
COPY ["src/EntityTagCaching/Directory.Build.props", "src/EntityTagCaching/"]
COPY ["Directory.Build.targets", "."]
COPY ["samples/DotNetRestAPI/DotNetRestAPI.csproj", "samples/DotNetRestAPI/"]
COPY ["src/HypermediaEngine/AspNetCore/HypermediaEngine.AspNetCore.csproj", "src/HypermediaEngine/AspNetCore/"]
COPY ["src/EntityTagCaching/Core/EntityTagCaching.Core.csproj", "src/EntityTagCaching/Core/"]
COPY ["src/HypermediaEngine/Core/HypermediaEngine.Core.csproj", "src/HypermediaEngine/Core/"]
RUN dotnet restore "./samples/DotNetRestAPI/DotNetRestAPI.csproj"
COPY . .
WORKDIR "/src/samples/DotNetRestAPI"
RUN dotnet build "./DotNetRestAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DotNetRestAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotNetRestAPI.dll"]
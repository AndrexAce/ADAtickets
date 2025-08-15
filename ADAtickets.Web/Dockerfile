# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8081
LABEL org.opencontainers.image.source=https://github.com/andrexace/adatickets

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ADAtickets.Web/ADAtickets.Web.csproj", "ADAtickets.Web/"]
COPY ["ADAtickets.Client/ADAtickets.Client.csproj", "ADAtickets.Client/"]
COPY ["ADAtickets.Shared/ADAtickets.Shared.csproj", "ADAtickets.Shared/"]
RUN dotnet restore "./ADAtickets.Web/ADAtickets.Web.csproj"
COPY . .
WORKDIR "/src/ADAtickets.Web"
RUN dotnet build "./ADAtickets.Web.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ADAtickets.Web.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ADAtickets.Web.dll"]
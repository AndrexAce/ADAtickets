FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
LABEL org.opencontainers.image.source=https://github.com/andrexace/adatickets

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ADAtickets.Api/ADAtickets.Api.csproj", "ADAtickets.Api/"]
COPY ["ADAtickets.ServiceDefaults/ADAtickets.ServiceDefaults.csproj", "ADAtickets.ServiceDefaults/"]
COPY ["ADAtickets.Shared/ADAtickets.Shared.csproj", "ADAtickets.Shared/"]
RUN dotnet restore "ADAtickets.Api/ADAtickets.Api.csproj"
COPY . .
WORKDIR "/src/ADAtickets.Api"
RUN dotnet build "./ADAtickets.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ADAtickets.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ADAtickets.Api.dll"]

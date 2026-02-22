FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["tools/GlobalEntryTrackerApiSeed/GlobalEntryTrackerApiSeed.csproj", "tools/GlobalEntryTrackerApiSeed/"]
COPY ["Database/Database.csproj", "Database/"]

# Restore dependencies
RUN dotnet restore "tools/GlobalEntryTrackerApiSeed/GlobalEntryTrackerApiSeed.csproj"

# Copy source code
COPY ["tools/GlobalEntryTrackerApiSeed/", "tools/GlobalEntryTrackerApiSeed/"]
COPY ["Database/", "Database/"]

# Build
WORKDIR "/src/tools/GlobalEntryTrackerApiSeed"
RUN dotnet build "GlobalEntryTrackerApiSeed.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GlobalEntryTrackerApiSeed.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GlobalEntryTrackerApiSeed.dll"]

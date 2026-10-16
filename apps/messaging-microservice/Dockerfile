# Build context is the repository root, because the service references projects in packages/.
# Build from the repo root:
#   docker build -f apps/messaging-microservice/Dockerfile -t bizcord-messaging .

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the project files first so the restore layer is cached until a dependency changes.
# The monorepo directory structure is kept so the relative ProjectReferences still resolve.
COPY ["apps/messaging-microservice/src/Messaging.csproj", "apps/messaging-microservice/src/"]
COPY ["packages/MessageClient/MessageClient.csproj", "packages/MessageClient/"]
COPY ["packages/Shared.Contracts/Shared.Contracts.csproj", "packages/Shared.Contracts/"]
RUN dotnet restore "apps/messaging-microservice/src/Messaging.csproj"

COPY ["apps/messaging-microservice/src/", "apps/messaging-microservice/src/"]
COPY ["packages/MessageClient/", "packages/MessageClient/"]
COPY ["packages/Shared.Contracts/", "packages/Shared.Contracts/"]
WORKDIR "/src/apps/messaging-microservice/src"
RUN dotnet build "Messaging.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Messaging.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bizcord.Messaging.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
# Install curl
RUN apt-get update \
	&& apt-get install -y curl \
	&& rm -rf /var/lib/apt/lists/*
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
# Copy local common packages
COPY ./nupkgs/ /nupkgs/
# Restore
WORKDIR /src/
COPY push-updates-hub/nuget.config .
COPY push-updates-hub/src/PushUpdatesHub.Api/PushUpdatesHub.Api.csproj PushUpdatesHub.Api/
COPY push-updates-hub/src/PushUpdatesHub.Common/PushUpdatesHub.Common.csproj PushUpdatesHub.Common/
RUN dotnet restore --configfile nuget.config PushUpdatesHub.Api/PushUpdatesHub.Api.csproj
# Build
COPY push-updates-hub/src/PushUpdatesHub.Api/ PushUpdatesHub.Api/
COPY push-updates-hub/src/PushUpdatesHub.Common/ PushUpdatesHub.Common/
RUN dotnet build --configuration release --no-self-contained ./PushUpdatesHub.Api/PushUpdatesHub.Api.csproj

FROM build AS publish
# Publish
RUN dotnet publish --configuration release --no-build --no-self-contained --output /app/ ./PushUpdatesHub.Api/PushUpdatesHub.Api.csproj

FROM base AS final
WORKDIR /app/
COPY --from=publish /app/ .
ENTRYPOINT dotnet PushUpdatesHub.Api.dll
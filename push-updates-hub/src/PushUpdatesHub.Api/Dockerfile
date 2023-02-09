FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN apt-get update \
	&& apt-get install -y curl \
	&& rm -rf /var/lib/apt/lists/*
WORKDIR /app/
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /nupkgs/
COPY nupkgs/ .
WORKDIR /src/
COPY push-updates-hub/nuget.config .
COPY push-updates-hub/src/PushUpdatesHub.Api/PushUpdatesHub.Api.csproj PushUpdatesHub.Api/
COPY push-updates-hub/src/PushUpdatesHub.Common/PushUpdatesHub.Common.csproj PushUpdatesHub.Common/
RUN dotnet restore PushUpdatesHub.Api/PushUpdatesHub.Api.csproj
COPY push-updates-hub/src/PushUpdatesHub.Api/ PushUpdatesHub.Api/
COPY push-updates-hub/src/PushUpdatesHub.Common/ PushUpdatesHub.Common/
WORKDIR /src/PushUpdatesHub.Api/
RUN dotnet build PushUpdatesHub.Api.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish PushUpdatesHub.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app/
COPY --from=publish /app/publish .
ENTRYPOINT dotnet PushUpdatesHub.Api.dll
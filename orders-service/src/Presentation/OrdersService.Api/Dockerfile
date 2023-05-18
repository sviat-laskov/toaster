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
COPY orders-service/nuget.config .
COPY orders-service/src/Presentation/OrdersService.Api/OrdersService.Api.csproj OrdersService.Api/
COPY orders-service/src/Infrastructure/OrdersService.Infrastructure.ElasticSearch/OrdersService.Infrastructure.ElasticSearch.csproj OrdersService.Infrastructure.ElasticSearch/
COPY orders-service/src/Infrastructure/OrdersService.Infrastructure.MongoDB/OrdersService.Infrastructure.MongoDB.csproj OrdersService.Infrastructure.MongoDB/
COPY orders-service/src/Core/OrdersService.Application/OrdersService.Application.csproj OrdersService.Application/
COPY orders-service/src/Core/OrdersService.Domain/OrdersService.Domain.csproj OrdersService.Domain/
RUN dotnet restore --configfile nuget.config OrdersService.Api/OrdersService.Api.csproj
# Build
COPY orders-service/nuget.config .
COPY orders-service/src/Presentation/OrdersService.Api/ OrdersService.Api/
COPY orders-service/src/Infrastructure/OrdersService.Infrastructure.ElasticSearch/ OrdersService.Infrastructure.ElasticSearch/
COPY orders-service/src/Infrastructure/OrdersService.Infrastructure.MongoDB/ OrdersService.Infrastructure.MongoDB/
COPY orders-service/src/Core/OrdersService.Application/ OrdersService.Application/
COPY orders-service/src/Core/OrdersService.Domain/ OrdersService.Domain/
RUN dotnet build --configuration release --no-self-contained ./OrdersService.Api/OrdersService.Api.csproj

FROM build AS publish
# Publish
RUN dotnet publish --configuration release --no-build --no-self-contained --output /app/ ./OrdersService.Api/OrdersService.Api.csproj

FROM base AS final
WORKDIR /app/
COPY --from=publish /app/ .
ENTRYPOINT dotnet OrdersService.Api.dll
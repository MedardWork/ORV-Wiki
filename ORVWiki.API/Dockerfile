# syntax=docker/dockerfile:1.6
# Build context is the solution root (see docker-compose.yml).

ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Copy only csproj/sln first so `restore` is cached as long as deps don't change.
COPY ORV_Wiki_Backend.sln ./
COPY ORVWiki.API/ORVWiki.API.csproj ORVWiki.API/
COPY ORVWiki.Application/ORVWiki.Application.csproj ORVWiki.Application/
COPY ORVWiki.Infrastructure/ORVWiki.Infrastructure.csproj ORVWiki.Infrastructure/
RUN dotnet restore ORVWiki.API/ORVWiki.API.csproj

# Now copy the rest and publish a self-contained framework-dependent build.
COPY ORVWiki.API/ ORVWiki.API/
COPY ORVWiki.Application/ ORVWiki.Application/
COPY ORVWiki.Infrastructure/ ORVWiki.Infrastructure/
RUN dotnet publish ORVWiki.API/ORVWiki.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app

# The aspnet base image already ships a non-root `app` user — just switch
# to it for runtime (defense in depth, no perms change to /app needed since
# the COPY below sets ownership).
USER app

COPY --from=build --chown=app:app /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_NOLOGO=true
EXPOSE 8080

ENTRYPOINT ["dotnet", "ORVWiki.API.dll"]

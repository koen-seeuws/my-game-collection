# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so the layer is cached as long as the project files don't change.
COPY Directory.Build.props ./
COPY src/MyGameCollection.Domain/MyGameCollection.Domain.csproj src/MyGameCollection.Domain/
COPY src/MyGameCollection.Application.Abstractions/MyGameCollection.Application.Abstractions.csproj src/MyGameCollection.Application.Abstractions/
COPY src/MyGameCollection.Application/MyGameCollection.Application.csproj src/MyGameCollection.Application/
COPY src/MyGameCollection.Infrastructure.Persistence/MyGameCollection.Infrastructure.Persistence.csproj src/MyGameCollection.Infrastructure.Persistence/
COPY src/MyGameCollection.Infrastructure.Storage/MyGameCollection.Infrastructure.Storage.csproj src/MyGameCollection.Infrastructure.Storage/
COPY src/MyGameCollection.Infrastructure.Barcodes/MyGameCollection.Infrastructure.Barcodes.csproj src/MyGameCollection.Infrastructure.Barcodes/
COPY src/MyGameCollection.Infrastructure.Metadata/MyGameCollection.Infrastructure.Metadata.csproj src/MyGameCollection.Infrastructure.Metadata/
COPY src/MyGameCollection.Web/MyGameCollection.Web.csproj src/MyGameCollection.Web/
RUN dotnet restore src/MyGameCollection.Web/MyGameCollection.Web.csproj

COPY src/ src/
RUN dotnet publish src/MyGameCollection.Web/MyGameCollection.Web.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    PhotoStorage__RootPath=/data/photos \
    DataProtection__KeysPath=/data/keys

# The base image runs as the non-root "app" user (UID 1654); give it the data folder.
USER root
RUN mkdir -p /data/photos /data/keys && chown -R app:app /data
USER app

COPY --from=build /app/publish .

VOLUME ["/data"]
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD ["dotnet", "MyGameCollection.Web.dll", "--healthcheck"]

ENTRYPOINT ["dotnet", "MyGameCollection.Web.dll"]

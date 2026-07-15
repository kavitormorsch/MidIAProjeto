# syntax=docker/dockerfile:1.7

# ---------- runtime base ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the project file first to cache the restore layer.
COPY ["MidIAProjeto.csproj", "./"]
RUN dotnet restore "MidIAProjeto.csproj"

# Now copy the rest of the source.
COPY . .

RUN dotnet build "./MidIAProjeto.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    --no-restore

# ---------- publish ----------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MidIAProjeto.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---------- final image ----------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY --chmod=0755 efbundle /app/efbundle

COPY entrypoint.sh /app/entrypoint.sh

# .NET 10 images default to listening on 8080 as non-root.
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
    
ENTRYPOINT ["/app/entrypoint.sh"]

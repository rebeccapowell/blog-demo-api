# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore Blog-Demo-Api/Blog-Demo-Api.csproj \
 && dotnet publish Blog-Demo-Api/Blog-Demo-Api.csproj -c Release -o /app/out --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out ./

# (optional) install curl for HEALTHCHECK
USER root
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

# Expose docs only; ingress decides what to publish
EXPOSE 8080
EXPOSE 8081

# Docker health — checks INTERNAL mgmt port (not exposed publicly)
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD curl -fsS http://127.0.0.1:8081/health || exit 1

# If you prefer non-root at runtime:
# RUN useradd -m appuser && chown -R appuser:appuser /app
# USER appuser

ENTRYPOINT ["dotnet", "Blog-Demo-Api.dll"]

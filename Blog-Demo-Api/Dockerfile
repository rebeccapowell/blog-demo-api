﻿FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# needed to add wget, even though curl should have been enough.
RUN apt-get update && apt-get install -y \
    curl \
    wget
USER $APP_UID
WORKDIR /app
EXPOSE 4317
EXPOSE 4318

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Blog-Demo-Api/Blog-Demo-Api.csproj", "Blog-Demo-Api/"]
RUN dotnet restore "Blog-Demo-Api/Blog-Demo-Api.csproj"
COPY . .
WORKDIR "/src/Blog-Demo-Api"
RUN dotnet build "Blog-Demo-Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Blog-Demo-Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# override the stadard 8080 and use a separate port for internal healthchecks
ENV ASPNETCORE_URLS=http://0.0.0.0:4317;http://0.0.0.0:4318

# coolify picks this up
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 CMD curl --fail http://localhost:4318/healthz || exit

ENTRYPOINT ["dotnet", "Blog-Demo-Api.dll"]

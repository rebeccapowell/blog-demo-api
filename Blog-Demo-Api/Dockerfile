FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update && apt-get install -y curl
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

# coolify has it's own way
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 CMD curl --fail http://127.0.0.1:4318/healthz || exit

ENTRYPOINT ["dotnet", "Blog-Demo-Api.dll"]

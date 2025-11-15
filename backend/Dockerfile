# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for dependency restoration
COPY ["HomeService.sln", "./"]
COPY ["src/HomeService.API/HomeService.API.csproj", "src/HomeService.API/"]
COPY ["src/HomeService.Application/HomeService.Application.csproj", "src/HomeService.Application/"]
COPY ["src/HomeService.Domain/HomeService.Domain.csproj", "src/HomeService.Domain/"]
COPY ["src/HomeService.Infrastructure/HomeService.Infrastructure.csproj", "src/HomeService.Infrastructure/"]

# Restore dependencies (cached layer)
RUN dotnet restore "src/HomeService.API/HomeService.API.csproj"

# Copy remaining source code
COPY . .

# Build the application
WORKDIR "/src/src/HomeService.API"
RUN dotnet build "HomeService.API.csproj" -c Release -o /app/build --no-restore

# ============================================
# Stage 2: Publish
# ============================================
FROM build AS publish
RUN dotnet publish "HomeService.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --no-build \
    /p:UseAppHost=false

# ============================================
# Stage 3: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks and CA certificates
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl ca-certificates && \
    rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && \
    useradd -r -g appuser appuser && \
    chown -R appuser:appuser /app

# Copy published application from publish stage
COPY --from=publish --chown=appuser:appuser /app/publish .

# Switch to non-root user
USER appuser

# Expose ports (8080 for HTTP, 8081 for HTTPS)
EXPOSE 8080
EXPOSE 8081

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true

# Health check configuration
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "HomeService.API.dll"]

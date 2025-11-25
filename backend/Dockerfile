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

# Install curl for health checks, CA certificates, and SQLite
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl ca-certificates sqlite3 && \
    rm -rf /var/lib/apt/lists/*

# Create directory for SQLite database with proper permissions
RUN mkdir -p /app/data && chmod 777 /app/data

# Create non-root user for security
RUN groupadd -r appuser && \
    useradd -r -g appuser appuser && \
    chown -R appuser:appuser /app

# Copy published application from publish stage
COPY --from=publish --chown=appuser:appuser /app/publish .

# Switch to non-root user
USER appuser

# Expose port 9000 (Fly.io will map this internally)
EXPOSE 9000

# Environment variables
ENV ASPNETCORE_URLS=http://0.0.0.0:9000 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    ConnectionStrings__DefaultConnection="Data Source=/app/data/homeservice.db"

# Health check configuration
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl --fail http://localhost:9000/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "HomeService.API.dll"]

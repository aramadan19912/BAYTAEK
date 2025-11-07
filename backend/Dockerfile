# Backend Dockerfile for .NET 8 Application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/HomeService.API/HomeService.API.csproj", "src/HomeService.API/"]
COPY ["src/HomeService.Application/HomeService.Application.csproj", "src/HomeService.Application/"]
COPY ["src/HomeService.Infrastructure/HomeService.Infrastructure.csproj", "src/HomeService.Infrastructure/"]
COPY ["src/HomeService.Domain/HomeService.Domain.csproj", "src/HomeService.Domain/"]

RUN dotnet restore "src/HomeService.API/HomeService.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/HomeService.API"
RUN dotnet build "HomeService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HomeService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HomeService.API.dll"]

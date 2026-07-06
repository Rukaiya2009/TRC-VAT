# Multi-stage build for Render (SRS §6.2). Runs the API from /api.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TRC.Domain/TRC.Domain.csproj                 TRC.Domain/
COPY TRC.Shared/TRC.Shared.csproj                 TRC.Shared/
COPY TRC.Application/TRC.Application.csproj        TRC.Application/
COPY TRC.Infrastructure/TRC.Infrastructure.csproj TRC.Infrastructure/
COPY TRC.API/TRC.API.csproj                        TRC.API/
RUN dotnet restore TRC.API/TRC.API.csproj

COPY . .
RUN dotnet publish TRC.API/TRC.API.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
# Render injects $PORT at runtime; shell form expands it (ENV lines would not).
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet TRC.API.dll"]

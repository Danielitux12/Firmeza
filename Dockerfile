# syntax=docker/dockerfile:1

# ---------- Etapa 1: publicar Firmeza.API (.NET 10) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend
WORKDIR /build

COPY src/Firmeza.Domain/ ./src/Firmeza.Domain/
COPY src/Firmeza.Application/ ./src/Firmeza.Application/
COPY src/Firmeza.Infrastructure/ ./src/Firmeza.Infrastructure/
COPY src/Firmeza.API/ ./src/Firmeza.API/
RUN dotnet restore src/Firmeza.API/Firmeza.API.csproj

RUN dotnet publish src/Firmeza.API/Firmeza.API.csproj \
    -c Release -o /app/publish --no-restore

# ---------- Etapa 2: imagen final ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

COPY --from=backend /app/publish ./

# Usuario sin privilegios
USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "Firmeza.API.dll"]
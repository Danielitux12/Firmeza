# syntax=docker/dockerfile:1

# ---------- Etapa 1: compilar el frontend Angular ----------
# Se replica la estructura de la solución (firmeza-frontend y Firmeza.Web) para que
# el outputPath de angular.json (../Firmeza.Web/wwwroot/dist) siga funcionando.
FROM node:22-alpine AS frontend
WORKDIR /build/firmeza-frontend

COPY firmeza-frontend/package*.json ./
RUN npm ci

COPY firmeza-frontend/ ./
RUN mkdir -p /build/Firmeza.Web/wwwroot \
    && npx ng build --configuration production

# ---------- Etapa 2: publicar el backend .NET ----------
# Ajusta el tag (10.0, ...) al TargetFramework de tu .csproj.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend
WORKDIR /build

COPY Firmeza.Domain/ ./Firmeza.Domain/
COPY Firmeza.Application/ ./Firmeza.Application/
COPY Firmeza.Infrastructure/ ./Firmeza.Infrastructure/
COPY Firmeza.Web/ ./Firmeza.Web/
RUN dotnet restore Firmeza.Web/Firmeza.csproj

# Copia el Angular ya compilado dentro de wwwroot/dist
COPY --from=frontend /build/Firmeza.Web/wwwroot/dist ./Firmeza.Web/wwwroot/dist

RUN dotnet publish Firmeza.Web/Firmeza.csproj \
    -c Release -o /app/publish --no-restore

# ---------- Etapa 3: imagen final ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

COPY --from=backend /app/publish ./

# Usuario sin privilegios (la imagen aspnet ya trae el usuario "app" desde .NET 8)
USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "Firmeza.dll"]
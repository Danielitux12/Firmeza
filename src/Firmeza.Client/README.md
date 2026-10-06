# Firmeza Frontend

Aplicación frontend Angular para la solución Firmeza.

## Proyecto

Este repositorio combina dos partes:

- Backend ASP.NET Core en `Firmeza/`
- Frontend Angular en `firmeza-frontend/`

## Requisitos

- .NET SDK 10+
- Node.js 20+
- npm

## Instalar dependencias

```bash
cd firmeza-frontend
npm install
```

## Ejecutar el backend

Desde la raíz del proyecto:

```bash
dotnet run --project Firmeza/Firmeza.csproj --urls http://127.0.0.1:5050
```

Si el puerto 5287 está ocupado, el proyecto no arrancará porque otra instancia ya está escuchando ese puerto. En ese caso se recomienda usar un puerto libre, como el 5050 del ejemplo anterior.

## Ejecutar el frontend

```bash
cd firmeza-frontend
ng serve
```

Luego abre la app en:

```text
http://localhost:4200/
```

La aplicación recarga automáticamente al guardar cambios.

## Compilar el frontend

```bash
cd firmeza-frontend
ng build
```

El resultado se genera en `Firmeza/wwwroot/dist` para que el backend pueda servir los assets del frontend.

## Compilar el backend

```bash
dotnet build Firmeza/Firmeza.csproj
```

## Solución de problemas comunes

### Error: address already in use

Este error aparece cuando el puerto configurado ya está siendo usado por otra app. Para solucionarlo:

```bash
ss -lntup | grep 5287
```

Si aparece un proceso escuchando en ese puerto, detén la instancia previa o usa otro puerto libre.

## Generar componentes

```bash
ng generate component nombre-del-componente
```

## Recursos útiles

- [Angular CLI Documentation](https://angular.dev/tools/cli)
- [ASP.NET Core Documentation](https://learn.microsoft.com/aspnet/core)

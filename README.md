# Firmeza Solution

Proyecto con arquitectura en capas siguiendo principios de Clean Architecture, compuesto por un backend ASP.NET Core y un frontend Angular.

## Estructura del proyecto

```text
FirmezaSolution/
├─ src/
│  ├─ Firmeza.Domain              # Reglas del negocio y entidades
│  ├─ Firmeza.Application         # Casos de uso y lógica de aplicación
│  ├─ Firmeza.Infrastructure      # Acceso a datos, EF Core, repositorios
│  └─ Firmeza.Web                 # ASP.NET Core MVC/Web API
├─ firmeza-frontend/              # Frontend Angular
├─ tests/                         # Pruebas del proyecto
├─ FirmezaSolution.sln            # Solución principal
├─ docker-compose.yml
├─ Dockerfile
├─ .gitignore
└─ README.md
```

## Capa Domain

La capa Domain contiene la lógica más importante del negocio:

- Entidades
- Reglas de validación
- Contratos de repositorios
- Lógica central del sistema

No debe depender de bases de datos, APIs ni del frontend.

## Capa Application

La capa Application orquesta los casos de uso del sistema, por ejemplo:

- registro de usuarios
- autenticación
- validación de reglas de negocio
- transformación de datos

## Capa Infrastructure

La capa Infrastructure implementa detalles técnicos:

- Entity Framework Core
- PostgreSQL
- repositorios concretos
- servicios externos

## Capa Web

La capa Web es la capa de presentación y entrada al sistema:

- Controladores MVC
- Views
- configuración de ASP.NET Core
- enrutamiento de la aplicación

---

## Requisitos

Antes de iniciar el proyecto asegúrate de tener instalado:

- .NET SDK 10
- Node.js y npm
- Angular CLI (si vas a levantar el frontend de forma independiente)
- PostgreSQL (si vas a usar la infraestructura con base de datos real)

---

## Ejecutar el backend

Desde la raíz del proyecto:

```bash
dotnet restore
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project src/Firmeza.Web
```

La aplicación normalmente queda disponible en:

```text
http://localhost:5287
```

---

## Ejecutar el frontend Angular

Desde la carpeta del frontend:

```bash
cd firmeza-frontend
npm install
npm start
```

O bien:

```bash
cd firmeza-frontend
npx ng serve
```

---

## Compilar la solución completa

```bash
dotnet build FirmezaSolution.sln
```

---

## Nota sobre puerto y ejecución

La app ASP.NET puede fallar si el puerto 5287 ya está ocupado. En ese caso puedes ejecutarla con otro puerto:

```bash
dotnet run --project src/Firmeza.Web --urls http://localhost:5290
```

---

## Convenciones sugeridas

- La capa Domain no debe referenciar a Web ni Infrastructure.
- Application puede depender de Domain.
- Infrastructure depende de Application y Domain.
- Web puede depender de Application e Infrastructure.

Esto mantiene una separación clara entre negocio, ejecución y tecnología.

---

## Estado actual

La solución se encuentra organizada en capas limpias y lista para continuar con:

- entidades del dominio
- casos de uso
- repositorios
- persistencia con PostgreSQL
- controladores y vistas
- integración frontend/backend

Si quieres, después puedo dejarte también un README más técnico con ejemplos de:

- login con usuarios,
- entidades del dominio,
- repositorios y servicios,
- y estructura exacta para cada proyecto.

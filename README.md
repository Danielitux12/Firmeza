# Firmeza - Sistema de Gestión de Clientes y Empresas

Plataforma empresarial desarrollada en **ASP.NET Core (.NET 10)** y **Angular (SPA Standalone)**, diseñada siguiendo los principios de arquitectura limpia en capas, orientada a la robustez, alta cohesión, bajo acoplamiento y libre de sobreingeniería.

Tanto el panel de **Administrador** como el portal de **Cliente** están unificados en una sola aplicación web SPA moderna y responsiva en **Angular** (`Firmeza.Web`), comunicándose mediante una **Web API RESTful** con autenticación JWT Bearer y persistencia en **PostgreSQL**.

---

## 🏛️ Estructura del Repositorio

```text
Firmeza/
├─ src/
│  ├─ Firmeza.Domain/          # Entidades centrales y reglas de negocio puras (Cliente, Empresa, EntityBase)
│  ├─ Firmeza.Application/     # DTOs, interfaces de servicio, validadores FluentValidation y perfiles AutoMapper
│  ├─ Firmeza.Infrastructure/  # AppDbContext (Npgsql + Identity), configuraciones de entidad, repositorios y exportadores (Excel, PDF, SMTP)
│  ├─ Firmeza.API/             # Web API RESTful (JWT Bearer, Swagger interactivo, Controllers)
│  └─ Firmeza.Web/             # Frontend SPA en Angular (Componentes Standalone, tema oscuro premium, Guards, Interceptors)
├─ tests/
│  └─ Firmeza.Tests/           # Suite de pruebas unitarias xUnit (AutoMapper, FluentValidation)
├─ .env.example                # Plantilla de variables de entorno (Base de datos, JWT, SMTP)
├─ .env                        # Variables locales de entorno (ignorado por Git)
├─ FirmezaSolution.sln         # Solución principal en .NET 10
├─ Dockerfile                  # Empaquetado Docker para Firmeza.API
├─ docker-compose.yml          # Orquestación de contenedores (PostgreSQL, Firmeza.API y Firmeza.Web)
├─ start-web.sh                # Script optimizado para ejecutar Angular con Node.js v22+
├─ start-client.sh             # Script de compatibilidad para el frontend
└─ README.md                   # Documentación técnica completa del proyecto
```

---

## 📦 Capas y Responsabilidades

### 1. `Firmeza.Domain`
- **Responsabilidad:** Define las entidades centrales del modelo de negocio, independientes de frameworks de persistencia.
- **Entidades:**
  - `EntityBase`: Clase abstracta base con clave primaria `Guid Id` y estado lógico `bool IsActive` con métodos de encapsulamiento `Activate()` y `Deactivate()`.
  - `Cliente`: Entidad para los clientes del sistema (`Name`, `DocumentNumber`, `Email`, `Phone`, `Address`, `BirthDate`, `UserId`).
  - `Empresa`: Entidad para aliados y empresas comerciales (`Name`, `Nit`, `Email`, `Phone`, `Address`).

---

### 2. `Firmeza.Application`
- **Responsabilidad:** Casos de uso, validaciones y transformaciones de datos.
- **Componentes clave:**
  - **Servicios:** `IClienteService`, `IEmpresaService`.
  - **Validadores:** `ClienteValidator` y `EmpresaValidator` construidos con **FluentValidation**.
  - **DTOs:** `ClienteDto`, `SaveClienteDto`, `EmpresaDto`, `SaveEmpresaDto`, `LoginRequestDto`, `AuthResponseDto`.
  - **Exportadores:** `IExcelExporter`, `IPdfExporter` e `IEmailSender`.
  - **Mapeos de Entidades:** Perfiles de **AutoMapper** ordenados primero con `ClienteMappingProfile` y posteriormente `EmpresaMappingProfile`.

---

### 3. `Firmeza.Infrastructure`
- **Responsabilidad:** Persistencia en PostgreSQL mediante EF Core, seguridad con ASP.NET Core Identity y servicios técnicos.
- **Mapeo y Configuraciones de Entidades:**
  - Las configuraciones de base de datos (`ClienteConfiguration` y `EmpresaConfiguration`) se aplican explícitamente en el orden solicitado (**primero Clientes y luego Empresas**):
    - `ClienteConfiguration`: Mapea a la tabla `clientes`, define longitud de campos, clave primaria y restricciones de unicidad en `DocumentNumber` y `Email`.
    - `EmpresaConfiguration`: Mapea a la tabla `empresas`, define longitud de campos, clave primaria y restricciones de unicidad en `Nit` y `Email`.
- **Migraciones EF Core:**
  - Historial de migraciones ordenado y sincronizado con el modelo en `AppDbContextModelSnapshot.cs`.
  - Migración automática al iniciar la API (`DatabaseSeeder.SeedAsync`) para garantizar que la base de datos esté lista sin intervención manual.
- **Servicios Técnicos:**
  - `ExcelExporter`: Generación de hojas `.xlsx` estilizadas mediante **EPPlus**.
  - `PdfExporter`: Generación de documentos PDF tabulares con cabeceras y paginación mediante **QuestPDF**.
  - `SmtpEmailSender`: Envío desacoplado de notificaciones por correo vía SMTP.

---

### 4. `Firmeza.API` (Web API RESTful)
- **Tipo:** ASP.NET Core Web API (.NET 10).
- **Seguridad:** Autenticación JWT Bearer y políticas de autorización por rol (`SoloAdministrador` y `SoloCliente`).
- **Swagger UI:** Documentación interactiva disponible en `/swagger` con autenticación Bearer integrada.
- **Controladores y Endpoints:**
  - **`AuthController` (`/api/auth`)**:
    - `POST /api/auth/login`: Autenticación con generación de token JWT.
    - `POST /api/auth/register`: Registro de nuevos clientes.
    - `GET /api/auth/me`: Consulta de perfil del usuario en sesión.
  - **`DashboardController` (`/api/dashboard`)**:
    - `GET /api/dashboard`: Métricas consolidadas (total de clientes activos/suspendidos, empresas y desglose por roles).
  - **`ClientesController` (`/api/clientes`)**:
    - `GET /api/clientes`: Búsqueda paginada con filtros de texto, rol (`Administrador` / `Cliente`) y estado (`active` / `inactive` / `all`).
    - `GET /api/clientes/{id}`: Obtener detalle por ID.
    - `POST /api/clientes`: Crear nuevo cliente.
    - `PUT /api/clientes/{id}`: Actualizar cliente existente.
    - `DELETE /api/clientes/{id}`: **Eliminación lógica / Suspensión** (desactiva al cliente y bloquea el acceso de Identity).
    - `PATCH /api/clientes/{id}/activate`: **Reactivación** (devuelve al cliente a la cartera activa y desbloquea Identity).
    - `DELETE /api/clientes/{id}/permanent`: **Eliminación física definitiva** (elimina registro y usuario de Identity).
    - `GET /api/clientes/export-excel`: Descarga de reporte en formato Excel (.xlsx).
    - `GET /api/clientes/export-pdf`: Descarga de reporte en formato PDF (.pdf).
  - **`EmpresasController` (`/api/empresas`)**:
    - `GET /api/empresas`: Listado paginado y búsqueda por NIT, nombre o correo.
    - `POST /api/empresas`: Registro de nueva empresa.
    - `PUT /api/empresas/{id}`: Actualización de datos corporativos.
    - `DELETE /api/empresas/{id}`: Eliminación/desactivación de empresa.
    - `GET /api/empresas/export-excel`: Reporte Excel (.xlsx) de empresas.
    - `GET /api/empresas/export-pdf`: Reporte PDF (.pdf) de empresas.

---

### 5. `Firmeza.Web` (Frontend SPA en Angular)
- **Tipo:** Single Page Application en Angular con arquitectura Standalone, diseño moderno Dark Glassmorphism y Bootstrap 5.3.
- **Seguridad en el Cliente:**
  - `authInterceptor`: Inyecta el token Bearer en todas las peticiones HTTP y maneja errores de autenticación.
  - `authGuard` y `roleGuard`: Protegen rutas para evitar accesos no autorizados.
- **Módulos Principales:**
  - **Página de Inicio (`/`):** Landing page corporativa moderna con acceso rápido a login y portal.
  - **Autenticación (`/Login`):** Formulario de login con selector rápido de credenciales demo y validaciones en vivo.
  - **Panel de Administración (`/admin`):**
    - **Dashboard (`/admin`):** Tarjetas de métricas interactivas y acceso rápido.
    - **Gestión de Clientes (`/admin/clientes`):**
      - 📁 **Pestaña Cartera Activa:** Clientes operativos, creación, edición, filtro por rol y suspensión.
      - ⏸️ **Pestaña Clientes Suspendidos:** Visualización de clientes inactivos con opciones de **Reactivar** o **Eliminar Permanente**.
      - Exportación de reportes a **Excel** y **PDF**.
    - **Gestión de Empresas (`/admin/empresas`):**
      - Directorio empresarial, creación, edición, eliminación y exportación a **Excel** y **PDF**.
  - **Portal del Cliente (`/cliente`):**
    - Vista personalizada para clientes autenticados con consulta de perfil e información de contacto.

---

## 🚀 Cómo Ejecutar el Proyecto

### 1. Iniciar la Base de Datos (PostgreSQL)
Inicia el contenedor de base de datos con Docker Compose:
```bash
docker compose up -d db
```

### 2. Iniciar el Backend (`Firmeza.API`)
```bash
dotnet run --project src/Firmeza.API/Firmeza.API.csproj --launch-profile http
```
- **Swagger UI:** [http://localhost:5246/swagger](http://localhost:5246/swagger)

### 3. Iniciar el Frontend (`Firmeza.Web`)
Utiliza el script con entorno Node.js v22+ configurado:
```bash
./start-web.sh
```
- **Aplicación Web:** [http://localhost:4200](http://localhost:4200)

---

## 🔑 Credenciales de Acceso Iniciales

| Rol | Correo Electrónico | Contraseña | Destino tras Login |
| :--- | :--- | :--- | :--- |
| **Administrador** | `admin@firmeza.com` | `Admin123*` | `/admin` (Dashboard Admin) |
| **Cliente** | `cliente@firmeza.com` | `Cliente123*` | `/cliente` (Portal Cliente) |

---

## 🧪 Pruebas Unitarias

Para ejecutar las pruebas automatizadas del proyecto:
```bash
dotnet test
```
Todas las pruebas de validación, reglas de dominio y perfiles de mapeo deben ejecutarse exitosamente.

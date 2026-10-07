# Firmeza - Sistema de Gestión de Clientes y Empresas

Proyecto desarrollado en **ASP.NET Core (.NET 10)** y **Angular**, diseñado siguiendo una arquitectura limpia en capas, enfocada en simplicidad, legibilidad y libre de sobreingeniería.

El núcleo del sistema gestiona el ciclo de vida completo y la administración de **Clientes** y **Empresas** (proveedores y aliados comerciales).

---

## 🏛️ Estructura del Repositorio

```text
Firmeza/
├─ src/
│  ├─ Firmeza.Domain/          # Entidades centrales y reglas puras de dominio
│  ├─ Firmeza.Application/     # DTOs, ViewModels, servicios, validadores y mapeos
│  ├─ Firmeza.Infrastructure/  # AppDbContext (Identity + Npgsql), repositorio genérico y servicios (Excel, PDF, SMTP)
│  ├─ Firmeza.Admin/           # Panel web MVC (Razor Views + Bootstrap 5) para Administradores
│  ├─ Firmeza.API/             # Web API REST (JWT, Swagger interactivo, AutoMapper)
│  └─ Firmeza.Client/          # SPA en Angular (interfaz de usuario)
├─ tests/
│  └─ Firmeza.Tests/           # Pruebas unitarias xUnit (AutoMapper, validaciones FluentValidation)
├─ .env.example                # Plantilla de variables de entorno (Base de datos, JWT, SMTP)
├─ .env                        # Variables locales (ignorado por Git)
├─ FirmezaSolution.sln         # Solución principal en .NET 10
└─ README.md                   # Documentación técnica del proyecto
```

---

## 📦 Capas y Responsabilidades

### 1. `Firmeza.Domain`
- **Responsabilidad:** Define las entidades esenciales del modelo de negocio, totalmente agnósticas a frameworks de persistencia o capas superiores.
- **Entidades:**
  - `EntityBase`: Clase base con identificador único `Guid Id` y estado lógico `bool IsActive` con métodos `Activate()` y `Deactivate()`.
  - `Cliente`: Representa un cliente del sistema (`Name`, `DocumentNumber`, `Email`, `Phone`, `Address`, `BirthDate`, `UserId`).
  - `Empresa`: Representa una empresa o aliado comercial (`Name`, `Nit`, `Email`, `Phone`, `Address`).

---

### 2. `Firmeza.Application`
- **Responsabilidad:** Orquesta la lógica de negocio, casos de uso, validaciones y transformaciones de datos.
- **Componentes clave:**
  - **Servicios:**
    - `IClienteService` / `ClienteService`: Operaciones completas de consulta y mutación para clientes.
    - `IEmpresaService` / `EmpresaService`: Operaciones completas de consulta y mutación para empresas.
  - **Validaciones:**
    - `ClienteValidator`: Reglas de validación declarativas con **FluentValidation** para creación/actualización de clientes.
    - `EmpresaValidator`: Validación declarativa de empresas (NIT, razón social, correo, etc.).
  - **Modelos:**
    - **DTOs:** `ClienteDto`, `SaveClienteDto`, `EmpresaDto`, `SaveEmpresaDto`, `LoginRequestDto`, `RegisterRequestDto`, `AuthResponseDto`.
    - **ViewModels:** Modelos de vista para MVC (`ClienteViewModel`, `ClienteFilterViewModel`, `EmpresaViewModel`, `EmpresaFilterViewModel`, `DashboardViewModel`, `LoginViewModel`).
  - **Mapeos:**
    - `ClienteMappingProfile` y `EmpresaMappingProfile` mediante **AutoMapper**.
  - **Contratos utilitarios:**
    - `IExcelExporter`: Exportación estructurada a formato `.xlsx` con EPPlus.
    - `IPdfExporter`: Exportación de reportes tabulares a PDF con QuestPDF.
    - `IEmailSender`: Envío desacoplado de correos electrónicos con soporte para adjuntos.
    - `IRepository<T>`: Contrato genérico de persistencia.

---

### 3. `Firmeza.Infrastructure`
- **Responsabilidad:** Acceso a datos, persistencia e integraciones técnicas externas.
- **Componentes:**
  - `AppDbContext`: Hereda de `IdentityDbContext<IdentityUser>` para soporte de autenticación y roles con PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`).
  - `Repository<T>`: Implementación concreta y genérica del patrón repositorio.
  - `ExcelExporter`: Generación de hojas de cálculo con EPPlus.
  - `PdfExporter`: Generación de documentos PDF con QuestPDF.
  - `SmtpEmailSender`: Envío de correos electrónicos vía protocolo SMTP.

---

### 4. `Firmeza.Admin` (Panel Web MVC)
- **Tipo:** Aplicación ASP.NET Core MVC con Razor Views y Bootstrap 5.
- **Seguridad:** Autenticación por Cookies (`[Authorize(Roles = "Administrador")]`).
- **Módulos:**
  - **Dashboard:** Resumen métrico de clientes y empresas registradas y activas.
  - **Gestión de Clientes:**
    - Listado con paginación, filtros de estado (`active`, `inactive`, `all`) y búsqueda por texto.
    - Creación y edición con conversión segura de edad (`int.Parse` con control de excepciones `try-catch`).
    - Suspensión y reactivación lógica.
    - Exportación de la cartera de clientes a formato Excel y PDF.
  - **Gestión de Empresas:**
    - Listado, búsqueda y filtros.
    - Registro y actualización con validación de unicidad de NIT y correo electrónico.
    - Suspensión y reactivación lógica.

---

### 5. `Firmeza.API` (Web API REST)
- **Tipo:** ASP.NET Core Web API.
- **Seguridad:** JWT Bearer Token y políticas de autorización (`SoloAdministrador`).
- **Documentación interactiva:** Swagger UI habilitado con soporte para autorización Bearer (`Authorize`).
- **Endpoints clave:**
  - **Autenticación:**
    - `POST /api/auth/register`: Registro de cuentas con bienvenida por correo.
    - `POST /api/auth/login`: Retorna token JWT con reclamos de rol y `ClienteId`.
  - **Clientes (`/api/Clientes`):**
    - `GET /api/Clientes`: Listado paginado con búsqueda y filtro de estado.
    - `GET /api/Clientes/{id:guid}`: Consulta por identificador `Guid`.
    - `POST /api/Clientes`: Creación validada con FluentValidation y control de duplicados.
    - `PUT /api/Clientes/{id:guid}`: Actualización validada.
    - `PATCH /api/Clientes/{id:guid}/suspend` y `PATCH /api/Clientes/{id:guid}/activate`: Suspensión y reactivación lógica.
  - **Empresas (`/api/Empresas`):**
    - `GET /api/Empresas`: Listado paginado con búsqueda y filtro de estado.
    - `GET /api/Empresas/{id:guid}`: Consulta por identificador `Guid`.
    - `POST /api/Empresas`: Creación validada.
    - `PUT /api/Empresas/{id:guid}`: Actualización validada.
    - `PATCH /api/Empresas/{id:guid}/suspend` y `PATCH /api/Empresas/{id:guid}/activate`: Suspensión y reactivación lógica.

---

### 6. `Firmeza.Tests`
- Pruebas unitarias automáticas con **xUnit**:
  - Mapeos de entidades y cálculo de edad mediante `AutoMapper`.
  - Validación de campos requeridos y formatos en clientes con `ClienteValidator` (FluentValidation).
  - Manejo de excepciones en conversión numérica de texto a edad.

---

## ⚙️ Configuración y Variables de Entorno

El proyecto lee la configuración desde el archivo `.env` en la raíz de la solución o desde variables de entorno del sistema operativo:

### Ejemplo de `.env`:
```env
# Conexión PostgreSQL
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=firmeza;Username=firmeza;Password=coder1234

# Credenciales del Administrador inicial (DatabaseSeeder)
ADMIN_EMAIL=admin@firmeza.com
ADMIN_PASSWORD=Admin123*

# Autenticación JWT para Firmeza.API
JWT_KEY=ClaveSuperSecretaFirmeza2026ParaTokenJWTConLongitudSegura123!
JWT_ISSUER=FirmezaAPI
JWT_AUDIENCE=FirmezaClient

# Configuración SMTP (Envío de correos)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=tu_correo@gmail.com
SMTP_PASSWORD=tu_app_password
SMTP_FROM_EMAIL=tu_correo@gmail.com
SMTP_FROM_NAME=Firmeza
```

---

## 🚀 Cómo Ejecutar el Proyecto

### Requisitos Previos
- **.NET SDK 10** instalado.
- **Node.js >= v22.22.3** (requerido para Angular CLI).
- **Docker & Docker Compose** (para la base de datos PostgreSQL o ejecución en contenedores).

---

### 1. Base de Datos (PostgreSQL con Docker)
Asegúrate de contar con el archivo `.env` configurado en la raíz del proyecto. Para iniciar el contenedor de PostgreSQL:

```bash
docker compose up -d db
```

---

### 2. Compilación y Pruebas Unitarias

```bash
# Compilar toda la solución
dotnet build FirmezaSolution.sln

# Ejecutar las pruebas unitarias
dotnet test tests/Firmeza.Tests/Firmeza.Tests.csproj
```

---

### 3. Ejecución de Componentes en Local

#### A. Panel Web de Administración (Admin MVC)
```bash
dotnet run --project src/Firmeza.Admin/Firmeza.Admin.csproj
```
- **URL:** [http://localhost:5287](http://localhost:5287) o [https://localhost:7066](https://localhost:7066)
- **Credenciales por defecto:**
  - **Usuario:** `admin@firmeza.com`
  - **Contraseña:** `Admin123*`

#### B. Web API REST
```bash
dotnet run --project src/Firmeza.API/Firmeza.API.csproj
```
- **Swagger UI:** [http://localhost:5246/swagger](http://localhost:5246/swagger) o [https://localhost:7269/swagger](https://localhost:7269/swagger)
- Autenticación con JWT disponible haciendo clic en **Authorize**.

#### C. Frontend Angular (Cliente SPA)
> Requiere Node.js `>= v22.22.3`.

Puedes iniciarlo directamente con el script automatizado (utiliza Node 22 automáticamente):
```bash
./start-client.sh
```

O manualmente:
```bash
cd src/Firmeza.Client
npm install
npm start
```
- **URL:** [http://localhost:4200](http://localhost:4200)

---

### 4. Ejecución Completa con Docker Compose (Opcional)

Para desplegar la base de datos y la aplicación administrativa en contenedores:

```bash
docker compose up --build
```
- **Acceso:** [http://localhost:8085](http://localhost:8085)


---

## 📄 Licencias de Terceros
- **EPPlus:** Configurado bajo contexto de licencia no comercial (`LicenseContext.NonCommercial`).
- **QuestPDF:** Configurado bajo licencia comunitaria (`LicenseType.Community`).

# Firmeza - Solución Integral de Ferretería

Proyecto académico desarrollado en **ASP.NET Core (.NET 10)** y **Angular**, diseñado siguiendo una arquitectura limpia en capas, simple, legible y libre de sobreingeniería.

---

## 🏛️ Estructura del Repositorio

```text
Firmeza/
├─ src/
│  ├─ Firmeza.Domain/          # Entidades centrales y reglas puras de dominio
│  ├─ Firmeza.Application/     # DTOs, ViewModels, interfaces y perfiles AutoMapper
│  ├─ Firmeza.Infrastructure/  # AppDbContext (Identity + Npgsql), servicios (Excel, PDF, SMTP)
│  ├─ Firmeza.Admin/           # Panel web MVC (Razor Views + Bootstrap 5) para Administradores
│  ├─ Firmeza.API/             # Web API REST (JWT, Swagger interactivo, AutoMapper) para Clientes
│  └─ Firmeza.Client/          # SPA en Angular (interfaz para clientes de la tienda)
├─ tests/
│  └─ Firmeza.Tests/           # Pruebas unitarias xUnit (dominio, validaciones, mapeos)
├─ samples/
│  └─ ejemplo_importacion.xlsx # Plantilla Excel de ejemplo para importación masiva
├─ .env.example                # Plantilla de variables de entorno (Base de datos, JWT, SMTP)
├─ .env                        # Variables locales (ignorado por Git)
├─ FirmezaSolution.sln         # Solución principal en .NET 10
└─ README.md                   # Documentación técnica del proyecto
```

---

## 📦 Capas y Responsabilidades

### 1. `Firmeza.Domain`
- **Responsabilidad:** Contiene las entidades esenciales del negocio sin dependencias externas ni frameworks de persistencia.
- **Entidades:**
  - `Product`: Id, Name, Description, Price, Stock, Category, IsAvailable.
  - `Customer`: Id, FullName, DocumentNumber, Email, Phone, BirthDate, Address, UserId.
  - `Sale`: Id, SaleNumber, Date, CustomerId, Subtotal, Tax (IVA 19%), Total, ReceiptPath.
  - `SaleDetail`: Id, SaleId, ProductId, Quantity, UnitPrice, LineTotal.

### 2. `Firmeza.Application`
- **Responsabilidad:** Define contratos (`interfaces`), modelos de transferencia (`DTOs`), modelos de vista (`ViewModels`) y transformaciones con `AutoMapper`.
- **Contratos principales:**
  - `IExcelImporter`: Normalización y carga masiva de catálogos desnormalizados.
  - `IExcelExporter`: Exportación estructurada a formato `.xlsx` con EPPlus.
  - `IPdfExporter`: Exportación de reportes tabulares a PDF con QuestPDF.
  - `IReceiptGenerator`: Generación de comprobantes comerciales físicos y en memoria.
  - `IEmailSender`: Envío desacoplado de correos electrónicos con soporte para adjuntos.

### 3. `Firmeza.Infrastructure`
- **Responsabilidad:** Implementación técnica concreta y persistencia de datos.
- **Componentes:**
  - `AppDbContext`: Hereda de `IdentityDbContext<IdentityUser>` para soporte de autenticación y roles con PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`).
  - `DatabaseSeeder`: Siembra inicial automática de roles (`Administrador`, `Cliente`) y usuario admin.
  - Implementaciones concretas: `ExcelImporter`, `ExcelExporter`, `PdfExporter`, `ReceiptGenerator`, `SmtpEmailSender`.

### 4. `Firmeza.Admin` (Panel de Administración)
- **Tipo:** Aplicación ASP.NET Core MVC con Razor Views y Bootstrap 5.
- **Seguridad:** Autenticación por Cookies (`[Authorize(Roles = "Administrador")]`).
- **Módulos:**
  - **Dashboard:** Métricas y tarjetas de resumen en tiempo real.
  - **Productos:** CRUD, búsqueda, filtros de categoría y disponibilidad, exportación Excel y PDF.
  - **Clientes:** CRUD con validaciones de unicidad, conversión de edad y exportación.
  - **Ventas:** Registro interactivo con cálculo automático de IVA (19%), descuento de stock en almacén y descarga de recibo.
  - **Importación Masiva:** Carga de Excel desnormalizado con reporte de errores en pantalla y logs `.txt`.

### 5. `Firmeza.API` (API REST)
- **Tipo:** ASP.NET Core Web API.
- **Seguridad:** JWT Bearer Token y políticas de autorización (`SoloAdministrador`, `SoloCliente`).
- **Documentación interactiva:** Swagger UI habilitado con soporte para autorización Bearer (`Authorize`).
- **Endpoints clave:**
  - `POST /api/auth/register`: Registro de clientes, creación de usuario Identity y envío de correo de bienvenida.
  - `POST /api/auth/login`: Retorna token JWT con reclamos de rol y `CustomerId`.
  - `GET /api/products`: Catálogo de productos.
  - `POST /api/sales`: Registro de ventas para clientes con descuento de inventario, emisión de recibo PDF y envío de comprobante al correo vía SMTP.
  - `GET /api/sales/{id}/receipt`: Descarga protegida del comprobante en PDF.

### 6. `Firmeza.Tests`
- Pruebas unitarias automáticas con **xUnit** para verificar:
  - Validaciones de dominio (`Product`, `Customer`).
  - Cálculo de subtotales, IVA 19% y totales de ventas (`Sale`, `SaleDetail`).
  - Mapeos de entrada y salida con `AutoMapper`.

---

## ⚙️ Configuración y Variables de Entorno

El proyecto lee la configuración directamente desde un archivo `.env` en la raíz o desde variables del sistema operativo:

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

# Configuración SMTP (Gmail u otro servidor)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=tu_correo@gmail.com
SMTP_PASSWORD=tu_app_password
SMTP_FROM_EMAIL=tu_correo@gmail.com
SMTP_FROM_NAME=Firmeza Tienda
```

---

## 🚀 Cómo Ejecutar el Proyecto

### 1. Compilar toda la solución
```powershell
dotnet build FirmezaSolution.sln
```

### 2. Ejecutar las pruebas unitarias
```powershell
dotnet test tests/Firmeza.Tests/Firmeza.Tests.csproj
```

### 3. Ejecutar el Panel Web de Administración (Admin MVC)
```powershell
dotnet run --project src/Firmeza.Admin/Firmeza.Admin.csproj
```
- Acceso: `http://localhost:<puerto>/Account/Login`
- Usuario por defecto: `admin@firmeza.com` / `Admin123*`

### 4. Ejecutar la API REST (para Clientes y Swagger)
```powershell
dotnet run --project src/Firmeza.API/Firmeza.API.csproj
```
- Swagger UI interactivo: `http://localhost:<puerto>/swagger`
- Haz clic en **Authorize** para ingresar el token JWT obtenido en `/api/auth/login`.

---

## 📄 Licencias de Terceros
- **EPPlus:** Configurado bajo contexto de licencia no comercial (`LicenseContext.NonCommercial`).
- **QuestPDF:** Configurado bajo licencia comunitaria (`LicenseType.Community`).

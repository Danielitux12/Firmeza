# Firmeza Solution

Project based on a layered architecture with a clear separation between business, application, infrastructure, and presentation.

## Project structure

```text
FirmezaSolution/
├─ src/
│  ├─ Firmeza.Domain/              # Domain logic and entities
│  ├─ Firmeza.Application/         # Use cases, services, and contracts
│  ├─ Firmeza.Infrastructure/      # EF Core, PostgreSQL, repositories
│  └─ Firmeza.Web/                 # ASP.NET Core MVC/Web API
├─ firmeza-frontend/            # Angular frontend
├─ .env.example                 # Example environment variables
├─ .env                         # Local environment variables (do not commit)
├─ docker-compose.yml           # App and PostgreSQL orchestration
├─ Dockerfile                   # Backend image
├─ FirmezaSolution.sln          # Main solution
├─ README.md                    # Project documentation
├─ .gitignore
├─ .dockerignore
├─ .idea/                       # Local IDE configuration
└─ tests/                       # Test folder (if added later)
```

---

## Layered architecture

The solution is organized with the base structure requested by the team:

- Domain: business entities and rules
- Application: use cases and contracts
- Infrastructure: data access and technical details
- Web: controllers and HTTP exposure

Dependency direction is as follows:

- Domain depends on no one
- Application depends on Domain
- Infrastructure depends on Application and Domain
- Web depends on Application and infrastructure only through interfaces and configuration

This keeps the solution decoupled and easier to maintain, while still being a well-structured monolith.

---

## 1. Firmeza.Domain

### Path
`Firmeza.Domain`

### Responsibility
Contains the business entities, domain rules, and validations for the core business logic.

### Includes
- entities
- business validations
- state rules
- core logic without depending on databases or APIs

### Example
- `Employee`
- `Product`
- validations such as `IsValid()`, `Activate()`, `Deactivate()`

### Key rule
It must not depend on:
- ASP.NET Core
- Entity Framework
- Angular
- PostgreSQL

---

## 2. Firmeza.Application

### Path
`Firmeza.Application`

### Responsibility
Coordinates the business use cases and defines the application logic.

### Includes
- services
- repository interfaces
- DTOs
- use cases
- application flow validation

### Example
- create employee
- list products
- validate data before persistence

### Dependencies
- depends on Domain
- should not depend directly on the Web layer

---

## 3. Firmeza.Infrastructure

### Path
`Firmeza.Infrastructure`

### Responsibility
Implements technical details such as database access, persistence, configuration, and external services.

### Includes
- `AppDbContext`
- entity configurations
- concrete repositories
- migrations
- dependency injection

### Example
- save employees in PostgreSQL
- implement repository interfaces
- map entities to database tables

### Dependencies
- depends on Domain and Application

---

## 4. Firmeza.Web

### Path
`Firmeza.Web`

### Responsibility
This is the entry and presentation layer of the system. It exposes the application through HTTP endpoints and renders the MVC UI.

### Includes
- controllers
- models
- views
- HTTP pipeline configuration
- app startup

### Example
- `HomeController`
- `LoginController`
- REST or MVC endpoints for screens

### Dependencies
- depends on Application and configured infrastructure

---

## 5. Angular frontend

### Path
`firmeza-frontend/`

### Responsibility
This is the user presentation layer. It consumes the backend API and displays the information.

### Includes
- components
- HTTP services
- routes
- styles
- templates

### Example
- login
- employee list
- product management

---

## Environment variables

The project uses environment variables for the PostgreSQL connection.

### Example file
`.env.example`

```env
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=firmeza;Username=firmeza;Password=coder1234
```

### Real local file
Create a `.env` file in the root with your real values.

---

## Requirements

Before running the project, make sure you have installed:

- .NET SDK 10
- Node.js and npm
- Angular CLI
- PostgreSQL or Docker

---

## Run the backend locally

From the project root:

```bash
dotnet restore
dotnet run --project src/Firmeza.Web
```

The application is usually available at:

```text
http://localhost:5287
```

If you need to force a specific port:

```bash
dotnet run --project src/Firmeza.Web --urls http://localhost:5290
```

---

## Run the Angular frontend

From the project root:

```bash
cd firmeza-frontend
npm install
npm start
```

Or directly:

```bash
cd firmeza-frontend
npx ng serve
```

The frontend normally runs at:

```text
http://localhost:4200
```

---

## Run with Docker

From the project root:

```bash
docker compose up --build
```

This starts:
- the web application
- PostgreSQL

The app is exposed at:

```text
http://localhost:8085
```

The database is available at:

```text
localhost:5432
```

---

## Build the complete solution

```bash
dotnet build FirmezaSolution.sln
```

---

## Suggested conventions

- Domain must not depend on Web or Infrastructure
- Application coordinates use cases and defines contracts
- Infrastructure implements those contracts with EF Core and PostgreSQL
- Web only exposes functionality to the client
- The frontend consumes the API and should not contain critical business logic

---

## Current state

The solution is organized in four main layers and is ready to continue with:

- domain entities
- application services
- infrastructure and persistence
- controllers and endpoints
- connection with Angular and Docker

If you want, I can also create a more technical README by layer with concrete examples for entities, services, repositories, and endpoints.

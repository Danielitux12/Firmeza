using System.Text;
using Firmeza.API.Configuration;
using Firmeza.Application;
using Firmeza.Application.Mappings;
using Firmeza.Infrastructure;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Carga las variables de entorno desde el archivo .env si existe.
EnvironmentConfiguration.LoadDotEnv();

var builder = WebApplication.CreateBuilder(args);

// Agrega servicios de controladores para los endpoints de la API.
builder.Services.AddControllers();

// Configura CORS para permitir solicitudes del cliente Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Agrega AutoMapper con los perfiles de Firmeza.Application
var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
{
    cfg.AddProfile<ClienteMappingProfile>();
    cfg.AddProfile<EmpresaMappingProfile>();
});
builder.Services.AddSingleton(mapperConfig.CreateMapper());

// Registra los validadores de FluentValidation de Application
builder.Services.AddApplicationServices();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Los datos enviados no son válidos.",
            Detail = "Corrige los campos indicados y vuelve a intentarlo."
        };
        return new BadRequestObjectResult(problem);
    };
});

// Configura la infraestructura y base de datos con la misma cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Falta configurar la cadena de conexión 'ConnectionStrings:DefaultConnection'. " +
        "Define la variable de entorno ConnectionStrings__DefaultConnection o agrégala en .env.");
}
builder.Services.AddInfrastructure(connectionString);

// Configuración de autenticación con JWT Bearer
var jwtKey = builder.Configuration["JWT_KEY"] ?? builder.Configuration["Jwt:Key"] ?? "ClaveSuperSecretaFirmeza2026ParaTokenJWTConLongitudSegura123!";
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? builder.Configuration["Jwt:Issuer"] ?? "FirmezaAPI";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? builder.Configuration["Jwt:Audience"] ?? "FirmezaClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var principal = context.Principal;
            if (principal?.IsInRole("Cliente") != true)
            {
                return;
            }

            if (!Guid.TryParse(principal.FindFirst("ClienteId")?.Value, out var clienteId))
            {
                context.Fail("La identidad de cliente no es válida.");
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var isActive = await dbContext.Clientes.AsNoTracking()
                .AnyAsync(cliente => cliente.Id == clienteId && cliente.IsActive, context.HttpContext.RequestAborted);
            if (!isActive)
            {
                context.Fail("La cuenta de cliente está suspendida.");
            }
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "No autenticado",
                Detail = "Se requiere un token de acceso válido."
            });
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Acceso denegado",
                Detail = "No tienes permisos para realizar esta operación."
            });
        }
    };
});

// Políticas de autorización solicitadas
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdministrador", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("SoloCliente", policy => policy.RequireRole("Cliente"));
});

// Configura Swagger / Swashbuckle con soporte para ingresar el token Bearer ("Authorize")
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Firmeza Store API",
        Version = "v1",
        Description = "API REST de Firmeza para clientes y administración."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa tu token JWT en este formato: Bearer {tu_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Aplica migraciones y ejecuta el seed inicial automáticamente si la BD está disponible
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
        if (dbContext is not null)
        {
            dbContext.Database.Migrate();
            DatabaseSeeder.SeedAsync(scope.ServiceProvider, app.Configuration).GetAwaiter().GetResult();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Aviso: No se pudo conectar a la base de datos PostgreSQL desde la API.");
    }
}

// Swagger UI para pruebas interactivas
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Firmeza Store API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Habilita CORS
app.UseCors("AllowAngularClient");

// Habilita autenticación y autorización en orden
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Firmeza.Application.DTOs.Auth;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Firmeza.API.Controllers;

/// <summary>
/// Controlador de autenticación para la API REST.
/// Maneja el registro de clientes y el inicio de sesión con tokens JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context,
        IConfiguration configuration,
        IEmailSender emailSender,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
        _emailSender = emailSender;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuevo usuario con rol 'Cliente' y crea su entidad Cliente asociada.
    /// Envía además un correo electrónico de bienvenida.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verifica si el correo ya está registrado en Identity o en Clientes
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "El correo electrónico ya se encuentra registrado." });
        }

        var existingCliente = await _context.Clientes.AnyAsync(c => c.DocumentNumber == request.DocumentNumber || c.Email == request.Email);
        if (existingCliente)
        {
            return BadRequest(new { message = "Ya existe un cliente con ese número de documento o correo electrónico." });
        }

        // 1. Crear el IdentityUser
        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.Phone
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new { message = $"Error al registrar usuario: {errors}" });
        }

        // 2. Asignar el rol "Cliente"
        if (!await _roleManager.RoleExistsAsync("Cliente"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Cliente"));
        }
        await _userManager.AddToRoleAsync(user, "Cliente");

        // 3. Crear el Cliente vinculado
        var cliente = new Cliente
        {
            Name = request.Name,
            DocumentNumber = request.DocumentNumber,
            Email = request.Email,
            Phone = request.Phone,
            BirthDate = request.Age > 0 ? DateTime.UtcNow.AddYears(-request.Age) : null,
            Address = request.Address,
            UserId = user.Id
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        // 4. Enviar correo de bienvenida en segundo plano
        _ = Task.Run(async () =>
        {
            try
            {
                var subject = "¡Bienvenido a Tienda Firmeza!";
                var body = $@"
                    <h2>¡Hola, {cliente.Name}!</h2>
                    <p>Tu cuenta ha sido creada exitosamente en <strong>Firmeza</strong>.</p>
                    <p>Ahora puedes acceder a nuestro catálogo de productos y realizar tus compras en línea.</p>
                    <br/>
                    <p>Atentamente,<br/>Equipo Firmeza</p>";
                await _emailSender.SendEmailAsync(cliente.Email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo enviar el correo de bienvenida a {Email}", cliente.Email);
            }
        });

        return Ok(new { message = "Usuario y cliente registrados exitosamente." });
    }

    /// <summary>
    /// Inicia sesión y genera un token JWT con los roles y reclamos del usuario.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Credenciales inválidas",
                Detail = "El correo o la contraseña no son válidos."
            });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Cliente";

        var clienteIdStr = string.Empty;
        if (primaryRole == "Cliente")
        {
            var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == user.Id || c.Email == user.Email);
            if (cliente is null || !cliente.IsActive)
            {
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Cuenta suspendida",
                    Detail = "La cuenta de cliente no existe o está suspendida."
                });
            }
            clienteIdStr = cliente.Id.ToString();
        }

        // Construir reclamos (Claims) del JWT
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, primaryRole)
        };

        if (!string.IsNullOrEmpty(clienteIdStr))
        {
            claims.Add(new Claim("ClienteId", clienteIdStr));
        }

        // Para compatibilidad con roles múltiples
        foreach (var role in roles)
        {
            if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var jwtKey = _configuration["JWT_KEY"] ?? _configuration["Jwt:Key"] ?? "ClaveSuperSecretaFirmeza2026ParaTokenJWTConLongitudSegura123!";
        var jwtIssuer = _configuration["JWT_ISSUER"] ?? _configuration["Jwt:Issuer"] ?? "FirmezaAPI";
        var jwtAudience = _configuration["JWT_AUDIENCE"] ?? _configuration["Jwt:Audience"] ?? "FirmezaClient";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(8);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return Ok(new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Email = user.Email ?? string.Empty,
            Role = primaryRole,
            Expiration = expires
        });
    }

    /// <summary>
    /// Retorna los datos de perfil del usuario autenticado actualmente.
    /// </summary>
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Cliente";

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        Cliente? cliente = null;
        if (role == "Cliente")
        {
            var clienteIdStr = User.FindFirst("ClienteId")?.Value;
            if (Guid.TryParse(clienteIdStr, out var cId))
            {
                cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cId);
            }
            else
            {
                cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Email == email);
            }
        }

        return Ok(new
        {
            Email = email,
            Role = role,
            Cliente = cliente != null ? new
            {
                Id = cliente.Id,
                Name = cliente.Name,
                DocumentNumber = cliente.DocumentNumber,
                Email = cliente.Email,
                Phone = cliente.Phone,
                Address = cliente.Address,
                IsActive = cliente.IsActive,
                Age = cliente.BirthDate.HasValue ? DateTime.UtcNow.Year - cliente.BirthDate.Value.Year : 0
            } : null
        });
    }
}

using System.Security.Claims;
using AutoMapper;
using Firmeza.Application.DTOs.Sales;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

/// <summary>
/// Controlador REST para ventas.
/// Permite a clientes registrar su propia venta y descargar su recibo.
/// Administradores pueden consultar todas las ventas y registrar para cualquier cliente.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IReceiptGenerator _receiptGenerator;
    private readonly IEmailSender _emailSender;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SalesController> _logger;

    public SalesController(
        AppDbContext context,
        IMapper mapper,
        IReceiptGenerator receiptGenerator,
        IEmailSender emailSender,
        IWebHostEnvironment env,
        ILogger<SalesController> logger)
    {
        _context = context;
        _mapper = mapper;
        _receiptGenerator = receiptGenerator;
        _emailSender = emailSender;
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Lista ventas. Si es Administrador, devuelve todas. Si es Cliente, solo devuelve las suyas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaleDto>>> GetAll()
    {
        var isAdmin = User.IsInRole("Administrador");
        var query = _context.Sales
            .Include(s => s.Cliente)
            .Include(s => s.SaleDetails)
                .ThenInclude(d => d.Product)
            .AsNoTracking();

        if (!isAdmin)
        {
            var clienteId = await GetCurrentClienteIdAsync();
            if (clienteId == null)
            {
                return Forbid();
            }
            query = query.Where(s => s.ClienteId == clienteId.Value);
        }

        var sales = await query.OrderByDescending(s => s.Date).ToListAsync();
        return Ok(_mapper.Map<List<SaleDto>>(sales));
    }

    /// <summary>
    /// Obtiene una venta por Id. Un cliente solo puede ver la suya; un administrador cualquiera.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SaleDto>> GetById(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.Cliente)
            .Include(s => s.SaleDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale == null)
        {
            return NotFound(new { message = $"Venta con id {id} no encontrada." });
        }

        var isAdmin = User.IsInRole("Administrador");
        if (!isAdmin)
        {
            var clienteId = await GetCurrentClienteIdAsync();
            if (clienteId == null || sale.ClienteId != clienteId.Value)
            {
                return Forbid();
            }
        }

        return Ok(_mapper.Map<SaleDto>(sale));
    }

    /// <summary>
    /// Registra una nueva venta. Calcula subtotal, IVA 19% y total, descuenta stock,
    /// genera el recibo PDF y envía correo con el recibo adjunto.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SaleDto>> Create([FromBody] CreateSaleDto request)
    {
        if (!ModelState.IsValid || request.Items == null || !request.Items.Any())
        {
            return BadRequest(new { message = "La venta debe contener al menos un producto." });
        }

        int clienteId;
        var isAdmin = User.IsInRole("Administrador");

        if (isAdmin)
        {
            if (!request.ClienteId.HasValue)
            {
                return BadRequest(new { message = "El administrador debe especificar el ClienteId." });
            }
            clienteId = request.ClienteId.Value;
        }
        else
        {
            // El cliente solo puede registrar para sí mismo
            var currentClienteId = await GetCurrentClienteIdAsync();
            if (currentClienteId == null)
            {
                return BadRequest(new { message = "No se encontró el perfil de cliente asociado a tu usuario." });
            }
            clienteId = currentClienteId.Value;
        }

        var cliente = await _context.Clientes.FindAsync(clienteId);
        if (cliente == null)
        {
            return NotFound(new { message = $"Cliente con id {clienteId} no existe." });
        }

        // Crear la venta
        var sale = new Sale
        {
            SaleNumber = $"VTA-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
            Date = DateTime.UtcNow,
            ClienteId = clienteId,
            Cliente = cliente
        };

        decimal subtotal = 0;

        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                return BadRequest(new { message = $"El producto con id {item.ProductId} no existe." });
            }

            if (product.Stock < item.Quantity)
            {
                return BadRequest(new { message = $"Stock insuficiente para el producto '{product.Name}'. Stock actual: {product.Stock}." });
            }

            // Descontar inventario
            product.Stock -= item.Quantity;

            var lineTotal = product.Price * item.Quantity;
            subtotal += lineTotal;

            sale.SaleDetails.Add(new SaleDetail
            {
                ProductId = product.Id,
                Product = product,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                LineTotal = lineTotal
            });
        }

        // IVA colombiano 19%
        var tax = Math.Round(subtotal * 0.19m, 2);
        var total = subtotal + tax;

        sale.Subtotal = subtotal;
        sale.Tax = tax;
        sale.Total = total;

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        // Generar recibo en PDF
        byte[]? pdfBytes = null;
        try
        {
            pdfBytes = _receiptGenerator.GenerateReceiptBytes(sale);
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var receiptsDir = Path.Combine(webRoot, "recibos");
            if (!Directory.Exists(receiptsDir))
            {
                Directory.CreateDirectory(receiptsDir);
            }

            var fileName = $"Recibo_{sale.SaleNumber}.pdf";
            var filePath = Path.Combine(receiptsDir, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

            sale.ReceiptPath = $"/recibos/{fileName}";
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar o guardar recibo PDF de la venta {SaleNumber}", sale.SaleNumber);
        }

        // Enviar correo con recibo PDF adjunto
        if (pdfBytes != null && !string.IsNullOrWhiteSpace(cliente.Email))
        {
            var emailTo = cliente.Email;
            var saleNum = sale.SaleNumber;
            var custName = cliente.Name;
            var saleTotal = sale.Total;

            _ = Task.Run(async () =>
            {
                try
                {
                    var subject = $"Comprobante de compra - Venta #{saleNum}";
                    var body = $@"
                        <h2>¡Gracias por tu compra, {custName}!</h2>
                        <p>Hemos registrado tu orden exitosamente con número <strong>{saleNum}</strong>.</p>
                        <p><strong>Total pagado:</strong> ${saleTotal:N2}</p>
                        <p>Adjunto a este correo encontrarás el comprobante formal en PDF.</p>
                        <br/>
                        <p>Atentamente,<br/>Equipo Firmeza</p>";

                    await _emailSender.SendEmailWithAttachmentAsync(emailTo, subject, body, pdfBytes, $"Recibo_{saleNum}.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo enviar el comprobante de compra por correo a {Email}", emailTo);
                }
            });
        }

        var dto = _mapper.Map<SaleDto>(sale);
        return CreatedAtAction(nameof(GetById), new { id = sale.Id }, dto);
    }

    /// <summary>
    /// Descarga el recibo en PDF de la venta.
    /// Un cliente solo puede descargar el recibo de su propia venta.
    /// </summary>
    [HttpGet("{id}/receipt")]
    public async Task<IActionResult> DownloadReceipt(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.Cliente)
            .Include(s => s.SaleDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale == null)
        {
            return NotFound(new { message = $"Venta con id {id} no encontrada." });
        }

        var isAdmin = User.IsInRole("Administrador");
        if (!isAdmin)
        {
            var clienteId = await GetCurrentClienteIdAsync();
            if (clienteId == null || sale.ClienteId != clienteId.Value)
            {
                return Forbid();
            }
        }

        // Si ya existe el archivo físico en wwwroot/recibos
        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        if (!string.IsNullOrWhiteSpace(sale.ReceiptPath))
        {
            var relative = sale.ReceiptPath.TrimStart('/', '\\');
            var fullPath = Path.Combine(webRoot, relative);
            if (System.IO.File.Exists(fullPath))
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                return File(bytes, "application/pdf", $"Recibo_{sale.SaleNumber}.pdf");
            }
        }

        // Si por alguna razón no estaba en disco, se genera en memoria
        var generatedBytes = _receiptGenerator.GenerateReceiptBytes(sale);
        return File(generatedBytes, "application/pdf", $"Recibo_{sale.SaleNumber}.pdf");
    }

    /// <summary>
    /// Obtiene el Id del Cliente correspondiente al usuario actualmente autenticado.
    /// </summary>
    private async Task<int?> GetCurrentClienteIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c =>
            (userId != null && c.UserId == userId) ||
            (email != null && c.Email == email));

        return cliente?.Id;
    }
}

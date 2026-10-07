using Firmeza.Application.Common;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace Firmeza.Admin.Controllers;

/// <summary>
/// Gestiona la importación masiva de datos desde archivos Excel (.xlsx) desnormalizados.
/// </summary>
[Authorize(Roles = "Administrador")]
public class ImportController : Controller
{
    private readonly IExcelImporter _excelImporter;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ImportController(
        IExcelImporter excelImporter,
        IWebHostEnvironment webHostEnvironment)
    {
        _excelImporter = excelImporter;
        _webHostEnvironment = webHostEnvironment;
    }

    // Muestra la pantalla de carga del archivo Excel y opciones.
    [HttpGet]
    public IActionResult Index()
    {
        return View(new ImportResult());
    }

    // Procesa el archivo Excel subido por el administrador.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Por favor selecciona un archivo Excel para importar.");
            return View("Index", new ImportResult());
        }

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (extension != ".xlsx")
        {
            ModelState.AddModelError(string.Empty, "Formato no válido. Solo se admiten archivos con extensión .xlsx");
            return View("Index", new ImportResult());
        }

        string webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        string logDirectory = Path.Combine(webRoot, "logs");

        using var stream = file.OpenReadStream();
        var result = await _excelImporter.ImportAsync(stream, logDirectory, HttpContext.RequestAborted);

        if (!result.HasErrors && result.SuccessRows > 0)
        {
            TempData["SuccessMessage"] = $"¡Importación exitosa! Se procesaron {result.SuccessRows} filas correctamente.";
        }
        else if (result.HasErrors && result.SuccessRows > 0)
        {
            TempData["WarningMessage"] = $"Se procesaron {result.SuccessRows} filas, pero se detectaron {result.Errors.Count} errores.";
        }
        else if (result.HasErrors)
        {
            TempData["ErrorMessage"] = $"No se pudo importar ningún dato debido a {result.Errors.Count} errores.";
        }

        return View("Index", result);
    }

    // Permite descargar el archivo .txt de log con el detalle de errores generado.
    [HttpGet]
    public async Task<IActionResult> DownloadLog(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest();
        }

        // Sanitiza el nombre de archivo para evitar Path Traversal
        fileName = Path.GetFileName(fileName);
        string webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        string fullPath = Path.Combine(webRoot, "logs", fileName);

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound("El archivo de log solicitado no existe o ha expirado.");
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        return File(fileBytes, "text/plain; charset=utf-8", fileName);
    }

    // Descarga un archivo Excel desnormalizado de ejemplo con datos de prueba válidos y formato esperado.
    [HttpGet]
    public IActionResult DownloadSample()
    {
        ExcelPackage.License.SetNonCommercialOrganization("Firmeza");
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("DatosImportacion");

        // Encabezados desnormalizados
        string[] headers = [
            "Cliente", "Documento", "Email", "Telefono", "Direccion",
            "Producto", "Descripcion", "Categoria", "Precio", "Stock", "Cantidad",
            "Fecha", "NumeroVenta"
        ];

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Font.Bold = true;
        }

        // Fila de ejemplo 1
        ws.Cells[2, 1].Value = "Ferretería El Progreso";
        ws.Cells[2, 2].Value = "900987654";
        ws.Cells[2, 3].Value = "contacto@elprogreso.com";
        ws.Cells[2, 4].Value = "3104567890";
        ws.Cells[2, 5].Value = "Carrera 7 # 45-10";
        ws.Cells[2, 6].Value = "Cemento Gris Tipo 1";
        ws.Cells[2, 7].Value = "Bolsa de 50kg para estructuras";
        ws.Cells[2, 8].Value = "Construcción";
        ws.Cells[2, 9].Value = 32000.00;
        ws.Cells[2, 10].Value = 200;
        ws.Cells[2, 11].Value = 10;
        ws.Cells[2, 12].Value = DateTime.Today.ToString("yyyy-MM-dd");
        ws.Cells[2, 13].Value = "IMP-001";

        // Fila de ejemplo 2
        ws.Cells[3, 1].Value = "Ferretería El Progreso";
        ws.Cells[3, 2].Value = "900987654";
        ws.Cells[3, 3].Value = "contacto@elprogreso.com";
        ws.Cells[3, 4].Value = "3104567890";
        ws.Cells[3, 5].Value = "Carrera 7 # 45-10";
        ws.Cells[3, 6].Value = "Varilla Corrugada 1/2 pulgada";
        ws.Cells[3, 7].Value = "Barra de acero de 6 metros";
        ws.Cells[3, 8].Value = "Acero";
        ws.Cells[3, 9].Value = 28500.00;
        ws.Cells[3, 10].Value = 150;
        ws.Cells[3, 11].Value = 20;
        ws.Cells[3, 12].Value = DateTime.Today.ToString("yyyy-MM-dd");
        ws.Cells[3, 13].Value = "IMP-001";

        // Fila de ejemplo 3
        ws.Cells[4, 1].Value = "Constructora Hábitat SAS";
        ws.Cells[4, 2].Value = "800123999";
        ws.Cells[4, 3].Value = "compras@habitat.com";
        ws.Cells[4, 4].Value = "3007654321";
        ws.Cells[4, 5].Value = "Avenida El Dorado # 68C-61";
        ws.Cells[4, 6].Value = "Pintura Vinilo Tipo 1 Blanca";
        ws.Cells[4, 7].Value = "Cuñete de 5 galones lavable";
        ws.Cells[4, 8].Value = "Pinturas";
        ws.Cells[4, 9].Value = 145000.00;
        ws.Cells[4, 10].Value = 50;
        ws.Cells[4, 11].Value = 4;
        ws.Cells[4, 12].Value = DateTime.Today.ToString("yyyy-MM-dd");
        ws.Cells[4, 13].Value = "IMP-002";

        ws.Cells.AutoFitColumns();

        var bytes = package.GetAsByteArray();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ejemplo_importacion.xlsx");
    }
}

using Firmeza.Application.Common;

namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato para la importación y normalización masiva desde hojas Excel desnormalizadas.
/// </summary>
public interface IExcelImporter
{
    // Procesa el flujo del archivo .xlsx, normaliza los datos y devuelve el resultado con errores si los hay.
    Task<ImportResult> ImportAsync(Stream fileStream, string logDirectory, CancellationToken cancellationToken = default);
}

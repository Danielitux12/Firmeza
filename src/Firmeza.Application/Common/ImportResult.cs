namespace Firmeza.Application.Common;

/// <summary>
/// Contiene el resumen de la importación masiva de datos desde archivo Excel.
/// </summary>
public class ImportResult
{
    // Total de filas leídas en el archivo (excluyendo encabezados).
    public int TotalRowsRead { get; set; }

    // Cantidad de filas procesadas e insertadas/actualizadas con éxito.
    public int SuccessRows { get; set; }

    // Cantidad de clientes creados o actualizados.
    public int CustomersProcessed { get; set; }

    // Cantidad de productos creados o actualizados.
    public int ProductsProcessed { get; set; }

    // Cantidad de ventas generadas.
    public int SalesCreated { get; set; }

    // Lista de errores detectados por fila y columna.
    public List<ImportRowError> Errors { get; set; } = new();

    // Ruta relativa al archivo de texto con el log detallado de errores.
    public string? LogFilePath { get; set; }

    // Indica si se produjeron errores durante la importación.
    public bool HasErrors => Errors.Count > 0;
}

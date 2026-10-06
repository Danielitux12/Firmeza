namespace Firmeza.Application.Common;

/// <summary>
/// Registra el detalle de un error ocurrido en una fila específica durante la importación Excel.
/// </summary>
public class ImportRowError
{
    // Número de fila en la hoja de cálculo donde ocurrió el problema (1-indexado).
    public int RowNumber { get; set; }

    // Nombre de la columna o campo afectado.
    public string ColumnName { get; set; } = string.Empty;

    // Mensaje descriptivo y amigable del error.
    public string ErrorMessage { get; set; } = string.Empty;
}

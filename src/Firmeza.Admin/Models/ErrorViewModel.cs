namespace Firmeza.Admin.Models;

// Modelo simple para mostrar información de errores no controlados en las vistas.
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
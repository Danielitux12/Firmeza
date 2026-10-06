using Firmeza.Domain.Entities;

namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato para la generación física de recibos/facturas de ventas en formato PDF mediante QuestPDF.
/// </summary>
public interface IReceiptGenerator
{
    // Genera el archivo PDF del recibo de venta y devuelve la ruta relativa web donde fue almacenado.
    Task<string> GenerateReceiptPdfAsync(Sale sale, string webRootPath, CancellationToken cancellationToken = default);

    // Genera los bytes del documento PDF en memoria
    byte[] GenerateReceiptBytes(Sale sale);
}

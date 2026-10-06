using Firmeza.Domain.Entities;

namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato para la exportación de listados de entidades del sistema hacia documentos PDF.
/// </summary>
public interface IPdfExporter
{
    // Genera el documento PDF con el catálogo de productos.
    byte[] ExportProducts(IEnumerable<Product> products);

    // Genera el documento PDF con el directorio de clientes.
    byte[] ExportCustomers(IEnumerable<Customer> customers);

    // Genera el documento PDF con el historial de ventas.
    byte[] ExportSales(IEnumerable<Sale> sales);
}

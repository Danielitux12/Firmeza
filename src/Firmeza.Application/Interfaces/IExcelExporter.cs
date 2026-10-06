using Firmeza.Domain.Entities;

namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato para la exportación de entidades del sistema hacia hojas de cálculo en formato Excel (.xlsx).
/// </summary>
public interface IExcelExporter
{
    // Genera el archivo Excel binario con el listado de productos.
    byte[] ExportProducts(IEnumerable<Product> products);

    // Genera el archivo Excel binario con el listado de clientes.
    byte[] ExportCustomers(IEnumerable<Customer> customers);

    // Genera el archivo Excel binario con el listado de ventas.
    byte[] ExportSales(IEnumerable<Sale> sales);
}

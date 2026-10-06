using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Firmeza.Infrastructure.Services;

/// <summary>
/// Exporta reportes tabulares de productos, clientes y ventas hacia documentos PDF profesionales mediante QuestPDF.
/// </summary>
public class PdfExporter : IPdfExporter
{
    public PdfExporter()
    {
        // Licencia comunitaria de QuestPDF.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // Genera el documento PDF con el catálogo de productos.
    public byte[] ExportProducts(IEnumerable<Product> products)
    {
        var list = products.ToList();
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().Element(c => RenderHeader(c, "CATÁLOGO DE PRODUCTOS"));

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // ID
                        columns.RelativeColumn(3);  // Nombre
                        columns.RelativeColumn(2);  // Categoría
                        columns.ConstantColumn(70);  // Precio
                        columns.ConstantColumn(50);  // Stock
                        columns.ConstantColumn(70);  // Estado
                    });

                    // Encabezados de tabla
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("ID").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Nombre").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Categoría").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Precio").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("Stock").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("Estado").Bold().FontSize(9);
                    });

                    // Filas
                    for (int i = 0; i < list.Count; i++)
                    {
                        var p = list[i];
                        var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        table.Cell().Background(bg).Padding(5).Text(p.Id.ToString()).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(p.Name).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(p.Category).FontSize(8);
                        table.Cell().Background(bg).Padding(5).AlignRight().Text($"${p.Price:N2}").FontSize(8);
                        table.Cell().Background(bg).Padding(5).AlignCenter().Text(p.Stock.ToString()).FontSize(8);
                        table.Cell().Background(bg).Padding(5).AlignCenter().Text(p.Status switch
                        {
                            ProductStatus.Available => "Disponible",
                            ProductStatus.Unavailable => "No disponible",
                            ProductStatus.Discontinued => "Descontinuado",
                            _ => "Desconocido"
                        }).FontSize(8);
                    }
                });

                page.Footer().Element(RenderFooter);
            });
        }).GeneratePdf();
    }

    // Genera el documento PDF con el listado de clientes.
    public byte[] ExportClientes(IEnumerable<Cliente> clientes)
    {
        var list = clientes.ToList();
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().Element(c => RenderHeader(c, "DIRECTORIO DE CLIENTES"));

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // ID
                        columns.RelativeColumn(3);  // Nombre
                        columns.RelativeColumn(2);  // Documento
                        columns.RelativeColumn(3);  // Email
                        columns.RelativeColumn(2);  // Teléfono
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("ID").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Nombre").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Documento").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Email").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Teléfono").Bold().FontSize(9);
                    });

                    for (int i = 0; i < list.Count; i++)
                    {
                        var c = list[i];
                        var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        table.Cell().Background(bg).Padding(5).Text(c.Id.ToString()).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(c.Name).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(c.DocumentNumber).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(c.Email).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(c.Phone).FontSize(8);
                    }
                });

                page.Footer().Element(RenderFooter);
            });
        }).GeneratePdf();
    }

    // Genera el documento PDF con el historial de ventas.
    public byte[] ExportSales(IEnumerable<Sale> sales)
    {
        var list = sales.ToList();
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().Element(c => RenderHeader(c, "HISTORIAL DE VENTAS"));

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(70);  // N° Venta
                        columns.ConstantColumn(80);  // Fecha
                        columns.RelativeColumn(3);  // Cliente
                        columns.RelativeColumn(2);  // Subtotal
                        columns.RelativeColumn(2);  // IVA (19%)
                        columns.RelativeColumn(2);  // Total
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("N° Venta").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Fecha").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Cliente").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Subtotal").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("IVA (19%)").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Total").Bold().FontSize(9);
                    });

                    for (int i = 0; i < list.Count; i++)
                    {
                        var s = list[i];
                        var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        table.Cell().Background(bg).Padding(5).Text(s.SaleNumber).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(s.Date.ToString("yyyy-MM-dd")).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(s.Cliente?.Name ?? "N/A").FontSize(8);
                        table.Cell().Background(bg).Padding(5).AlignRight().Text($"${s.Subtotal:N2}").FontSize(8);
                        table.Cell().Background(bg).Padding(5).AlignRight().Text($"${s.Tax:N2}").FontSize(8);
                        table.Cell().Background(bg).Padding(5).AlignRight().Text($"${s.Total:N2}").FontSize(8).Bold();
                    }
                });

                page.Footer().Element(RenderFooter);
            });
        }).GeneratePdf();
    }

    private void RenderHeader(IContainer container, string title)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("FIRMEZA MATERIALES Y FERRETERÍA").FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                column.Item().Text(title).FontSize(11).SemiBold().FontColor(Colors.Grey.Darken2);
            });
            row.ConstantItem(150).AlignRight().Column(col =>
            {
                col.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}").FontSize(8).FontColor(Colors.Grey.Darken1);
                col.Item().Text("Sistema de Gestión Admin").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void RenderFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text("Documento generado automáticamente por Firmeza Admin").FontSize(8).FontColor(Colors.Grey.Medium);
            row.RelativeItem().AlignRight().Text(x =>
            {
                x.Span("Página ").FontSize(8);
                x.CurrentPageNumber().FontSize(8);
                x.Span(" de ").FontSize(8);
                x.TotalPages().FontSize(8);
            });
        });
    }
}

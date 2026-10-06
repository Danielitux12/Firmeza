using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Firmeza.Infrastructure.Services;

/// <summary>
/// Genera el archivo físico del comprobante/factura de venta en formato PDF utilizando QuestPDF
/// y lo almacena en la carpeta pública wwwroot/recibos.
/// </summary>
public class ReceiptGenerator : IReceiptGenerator
{
    public ReceiptGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // Genera el documento PDF del recibo, lo guarda en disco y retorna la ruta pública relativa.
    public async Task<string> GenerateReceiptPdfAsync(Sale sale, string webRootPath, CancellationToken cancellationToken = default)
    {
        // 1. Asegura que el directorio exista en wwwroot/recibos
        string receiptsDir = Path.Combine(webRootPath, "recibos");
        Directory.CreateDirectory(receiptsDir);

        string fileName = $"recibo_{sale.SaleNumber}.pdf";
        string filePath = Path.Combine(receiptsDir, fileName);

        // 2. Construcción visual del recibo con QuestPDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5); // Formato de recibo comercial
                page.Margin(20);

                // Encabezado
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("FIRMEZA MATERIALES S.A.S.").FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
                            c.Item().Text("NIT: 900.123.456-7").FontSize(8).FontColor(Colors.Grey.Darken2);
                            c.Item().Text("Calle Principal #10-20, Ferretería Central").FontSize(8).FontColor(Colors.Grey.Darken2);
                            c.Item().Text("PBX: (601) 555-0199").FontSize(8).FontColor(Colors.Grey.Darken2);
                        });

                        row.ConstantItem(140).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(c =>
                        {
                            c.Item().AlignCenter().Text("COMPROBANTE DE VENTA").FontSize(8).Bold();
                            c.Item().AlignCenter().Text(sale.SaleNumber).FontSize(11).Bold().FontColor(Colors.Blue.Darken3);
                            c.Item().AlignCenter().Text($"Fecha: {sale.Date.ToLocalTime():dd/MM/yyyy}").FontSize(8);
                        });
                    });

                    col.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                });

                // Contenido: Datos del cliente y tabla de artículos
                page.Content().PaddingTop(10).Column(contentCol =>
                {
                    // Bloque de datos del cliente
                    contentCol.Item().Background(Colors.Grey.Lighten4).Padding(6).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Cliente: {sale.Cliente?.Name ?? "Venta al mostrador"}").FontSize(8).Bold();
                            c.Item().Text($"Documento: {sale.Cliente?.DocumentNumber ?? "-"}").FontSize(8);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Teléfono: {sale.Cliente?.Phone ?? "-"}").FontSize(8);
                            c.Item().Text($"Dirección: {sale.Cliente?.Address ?? "-"}").FontSize(8);
                        });
                    });

                    // Tabla de productos
                    contentCol.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(20);   // #
                            cols.RelativeColumn(4);   // Descripción
                            cols.ConstantColumn(40);   // Cant
                            cols.RelativeColumn(2);   // P. Unit
                            cols.RelativeColumn(2);   // Total
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("#").FontSize(8).Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Producto").FontSize(8).Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text("Cant").FontSize(8).Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).AlignRight().Text("P. Unit").FontSize(8).Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).AlignRight().Text("Total").FontSize(8).Bold();
                        });

                        int i = 1;
                        foreach (var item in sale.SaleDetails)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(i.ToString()).FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.Product?.Name ?? "Producto").FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(item.Quantity.ToString()).FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"${item.UnitPrice:N2}").FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"${item.LineTotal:N2}").FontSize(8);
                            i++;
                        }
                    });

                    // Totales: Subtotal, IVA 19% y Total
                    contentCol.Item().PaddingTop(8).AlignRight().Width(180).Column(totCol =>
                    {
                        totCol.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal:").FontSize(8);
                            r.RelativeItem().AlignRight().Text($"${sale.Subtotal:N2}").FontSize(8);
                        });
                        totCol.Item().Row(r =>
                        {
                            r.RelativeItem().Text("IVA (19%):").FontSize(8);
                            r.RelativeItem().AlignRight().Text($"${sale.Tax:N2}").FontSize(8);
                        });
                        totCol.Item().PaddingTop(2).BorderTop(1).BorderColor(Colors.Grey.Darken1).Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL:").FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                            r.RelativeItem().AlignRight().Text($"${sale.Total:N2}").FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                        });
                    });
                });

                // Pie de página
                page.Footer().Column(col =>
                {
                    col.Item().AlignCenter().Text("¡Gracias por confiar en Firmeza Materiales!").FontSize(7).Italic();
                    col.Item().AlignCenter().Text("Conserve este comprobante para cualquier reclamación de garantía.").FontSize(6).FontColor(Colors.Grey.Medium);
                });
            });
        });

        // 3. Guarda el archivo en disco
        byte[] pdfBytes = document.GeneratePdf();
        await File.WriteAllBytesAsync(filePath, pdfBytes, cancellationToken);

        // 4. Retorna la ruta relativa para consumo web
        return $"/recibos/{fileName}";
    }

    // Genera los bytes del documento PDF en memoria
    public byte[] GenerateReceiptBytes(Sale sale)
    {
        var document = BuildDocument(sale);
        return document.GeneratePdf();
    }

    // Método privado para construir el diseño visual del comprobante
    private IDocument BuildDocument(Sale sale)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5); // Formato de recibo comercial
                page.Margin(20);

                // Encabezado
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("FIRMEZA MATERIALES S.A.S.").FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
                            c.Item().Text("NIT: 900.123.456-7").FontSize(8).FontColor(Colors.Grey.Darken2);
                            c.Item().Text("Calle Principal #10-20, Ferretería Central").FontSize(8).FontColor(Colors.Grey.Darken2);
                            c.Item().Text("PBX: (601) 555-0199").FontSize(8).FontColor(Colors.Grey.Darken2);
                        });

                        row.ConstantItem(140).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(c =>
                        {
                            c.Item().AlignCenter().Text("COMPROBANTE DE VENTA").FontSize(8).Bold();
                            c.Item().AlignCenter().Text(sale.SaleNumber).FontSize(11).Bold().FontColor(Colors.Blue.Darken3);
                            c.Item().AlignCenter().Text($"Fecha: {sale.Date.ToLocalTime():dd/MM/yyyy}").FontSize(8);
                        });
                    });

                    col.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                });

                // Contenido: Datos del cliente y tabla de artículos
                page.Content().PaddingTop(10).Column(contentCol =>
                {
                    // Bloque de datos del cliente
                    contentCol.Item().Background(Colors.Grey.Lighten4).Padding(6).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Cliente: {sale.Cliente?.Name ?? "Venta al mostrador"}").FontSize(8).Bold();
                            c.Item().Text($"Documento: {sale.Cliente?.DocumentNumber ?? "-"}").FontSize(8);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Teléfono: {sale.Cliente?.Phone ?? "-"}").FontSize(8);
                            c.Item().Text($"Dirección: {sale.Cliente?.Address ?? "-"}").FontSize(8);
                        });
                    });

                    // Tabla de productos
                    contentCol.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(20);   // #
                            cols.RelativeColumn(4);   // Descripción
                            cols.ConstantColumn(40);   // Cant
                            cols.RelativeColumn(2);   // P. Unit
                            cols.RelativeColumn(2);   // Total
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("#").FontSize(8).Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Producto").FontSize(8).Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text("Cant").FontSize(8).Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).AlignRight().Text("P. Unit").FontSize(8).Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).AlignRight().Text("Total").FontSize(8).Bold();
                        });

                        int i = 1;
                        foreach (var item in sale.SaleDetails)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(i.ToString()).FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.Product?.Name ?? "Producto").FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(item.Quantity.ToString()).FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"${item.UnitPrice:N2}").FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"${item.LineTotal:N2}").FontSize(8);
                            i++;
                        }
                    });

                    // Totales: Subtotal, IVA 19% y Total
                    contentCol.Item().PaddingTop(8).AlignRight().Width(180).Column(totCol =>
                    {
                        totCol.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal:").FontSize(8);
                            r.RelativeItem().AlignRight().Text($"${sale.Subtotal:N2}").FontSize(8);
                        });
                        totCol.Item().Row(r =>
                        {
                            r.RelativeItem().Text("IVA (19%):").FontSize(8);
                            r.RelativeItem().AlignRight().Text($"${sale.Tax:N2}").FontSize(8);
                        });
                        totCol.Item().PaddingTop(2).BorderTop(1).BorderColor(Colors.Grey.Darken1).Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL:").FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                            r.RelativeItem().AlignRight().Text($"${sale.Total:N2}").FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                        });
                    });
                });

                // Pie de página
                page.Footer().Column(col =>
                {
                    col.Item().AlignCenter().Text("¡Gracias por confiar en Firmeza Materiales!").FontSize(7).Italic();
                    col.Item().AlignCenter().Text("Conserve este comprobante para cualquier reclamación de garantía.").FontSize(6).FontColor(Colors.Grey.Medium);
                });
            });
        });
    }
}

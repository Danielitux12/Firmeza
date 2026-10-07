using Firmeza.Application.DTOs.Clientes;
using Firmeza.Application.DTOs.Empresas;
using Firmeza.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Firmeza.Infrastructure.Services;

public class PdfExporter : IPdfExporter
{
    public PdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportClientes(IEnumerable<ClienteDto> clientes)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text("Reporte de Clientes").FontSize(20).SemiBold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Nombre").Bold();
                        header.Cell().Text("Documento").Bold();
                        header.Cell().Text("Email").Bold();
                        header.Cell().Text("Teléfono").Bold();
                    });

                    foreach (var c in clientes)
                    {
                        table.Cell().Text(c.Name);
                        table.Cell().Text(c.DocumentNumber);
                        table.Cell().Text(c.Email);
                        table.Cell().Text(c.Phone);
                    }
                });
            });
        }).GeneratePdf();
    }

    public byte[] ExportEmpresas(IEnumerable<EmpresaDto> empresas)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text("Reporte de Empresas").FontSize(20).SemiBold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Nombre").Bold();
                        header.Cell().Text("NIT").Bold();
                        header.Cell().Text("Email").Bold();
                        header.Cell().Text("Teléfono").Bold();
                    });

                    foreach (var e in empresas)
                    {
                        table.Cell().Text(e.Name);
                        table.Cell().Text(e.Nit);
                        table.Cell().Text(e.Email);
                        table.Cell().Text(e.Phone);
                    }
                });
            });
        }).GeneratePdf();
    }
}
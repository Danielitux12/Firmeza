using Firmeza.Application.DTOs.Clientes;
using Firmeza.Application.DTOs.Empresas;
using Firmeza.Application.Interfaces;
using OfficeOpenXml;

namespace Firmeza.Infrastructure.Services;

public class ExcelExporter : IExcelExporter
{
    public ExcelExporter()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public byte[] ExportClientes(IEnumerable<ClienteDto> clientes)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Clientes");

        worksheet.Cells[1, 1].Value = "Id";
        worksheet.Cells[1, 2].Value = "Nombre";
        worksheet.Cells[1, 3].Value = "Documento";
        worksheet.Cells[1, 4].Value = "Email";
        worksheet.Cells[1, 5].Value = "Teléfono";
        worksheet.Cells[1, 6].Value = "Edad";
        worksheet.Cells[1, 7].Value = "Dirección";

        int row = 2;
        foreach (var c in clientes)
        {
            worksheet.Cells[row, 1].Value = c.Id.ToString();
            worksheet.Cells[row, 2].Value = c.Name;
            worksheet.Cells[row, 3].Value = c.DocumentNumber;
            worksheet.Cells[row, 4].Value = c.Email;
            worksheet.Cells[row, 5].Value = c.Phone;
            worksheet.Cells[row, 6].Value = c.Age;
            worksheet.Cells[row, 7].Value = c.Address;
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] ExportEmpresas(IEnumerable<EmpresaDto> empresas)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Empresas");

        worksheet.Cells[1, 1].Value = "Id";
        worksheet.Cells[1, 2].Value = "Razón Social";
        worksheet.Cells[1, 3].Value = "NIT";
        worksheet.Cells[1, 4].Value = "Email";
        worksheet.Cells[1, 5].Value = "Teléfono";
        worksheet.Cells[1, 6].Value = "Dirección";

        int row = 2;
        foreach (var e in empresas)
        {
            worksheet.Cells[row, 1].Value = e.Id.ToString();
            worksheet.Cells[row, 2].Value = e.Name;
            worksheet.Cells[row, 3].Value = e.Nit;
            worksheet.Cells[row, 4].Value = e.Email;
            worksheet.Cells[row, 5].Value = e.Phone;
            worksheet.Cells[row, 6].Value = e.Address;
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }
}
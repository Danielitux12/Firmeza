using Firmeza.Application.DTOs.Clientes;
using Firmeza.Application.DTOs.Empresas;

namespace Firmeza.Application.Interfaces;

public interface IExcelExporter
{
    byte[] ExportClientes(IEnumerable<ClienteDto> clientes);
    byte[] ExportEmpresas(IEnumerable<EmpresaDto> empresas);
}
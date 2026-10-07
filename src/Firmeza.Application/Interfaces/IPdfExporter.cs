using Firmeza.Application.DTOs.Clientes;
using Firmeza.Application.DTOs.Empresas;

namespace Firmeza.Application.Interfaces;

public interface IPdfExporter
{
    byte[] ExportClientes(IEnumerable<ClienteDto> clientes);
    byte[] ExportEmpresas(IEnumerable<EmpresaDto> empresas);
}
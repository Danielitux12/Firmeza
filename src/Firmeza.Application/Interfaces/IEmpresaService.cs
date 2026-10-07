using Firmeza.Application.DTOs.Empresas;

namespace Firmeza.Application.Interfaces;

public interface IEmpresaService
{
    Task<IEnumerable<EmpresaDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EmpresaDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmpresaDto> CreateAsync(SaveEmpresaDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, SaveEmpresaDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
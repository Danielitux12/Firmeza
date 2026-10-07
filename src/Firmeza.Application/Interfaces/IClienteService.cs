using Firmeza.Application.DTOs.Clientes;

namespace Firmeza.Application.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClienteDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClienteDto> CreateAsync(SaveClienteDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, SaveClienteDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
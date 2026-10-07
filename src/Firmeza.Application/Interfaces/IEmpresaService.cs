using Firmeza.Domain.Entities;

namespace Firmeza.Application.Interfaces;

public interface IEmpresaService
{
    Task<IEnumerable<Empresa>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Empresa?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Empresa> CreateAsync(Empresa empresa, CancellationToken cancellationToken = default);
}
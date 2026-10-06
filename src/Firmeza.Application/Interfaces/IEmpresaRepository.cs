using Firmeza.Domain.Entities;

namespace Firmeza.Application.Interfaces;

public interface IEmpresaRepository
{
    Task<IEnumerable<Empresa>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Empresa?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Empresa> AddAsync(Empresa empresa, CancellationToken cancellationToken = default);
}
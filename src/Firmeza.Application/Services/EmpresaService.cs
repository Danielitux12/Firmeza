using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Services;

public class EmpresaService : IEmpresaService
{
    private readonly IEmpresaRepository _empresaRepository;

    public EmpresaService(IEmpresaRepository empresaRepository)
    {
        _empresaRepository = empresaRepository;
    }

    public Task<IEnumerable<Empresa>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _empresaRepository.GetAllAsync(cancellationToken);
    }

    public Task<Empresa?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _empresaRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Empresa> CreateAsync(Empresa empresa, CancellationToken cancellationToken = default)
    {
        if (!empresa.IsValid())
        {
            throw new InvalidOperationException("Empresa data is not valid.");
        }

        return await _empresaRepository.AddAsync(empresa, cancellationToken);
    }
}
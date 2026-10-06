using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

public class EmpresaRepository : IEmpresaRepository
{
    private readonly AppDbContext _context;

    public EmpresaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Empresa>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Empresas
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Empresa?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Empresas.FirstOrDefaultAsync(empresa => empresa.Id == id, cancellationToken);
    }

    public async Task<Empresa> AddAsync(Empresa empresa, CancellationToken cancellationToken = default)
    {
        await _context.Empresas.AddAsync(empresa, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return empresa;
    }
}
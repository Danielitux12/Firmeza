using Firmeza.Application.DTOs.Empresas;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Services;

public class EmpresaService : IEmpresaService
{
    private readonly IRepository<Empresa> _repository;

    public EmpresaService(IRepository<Empresa> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EmpresaDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var empresas = await _repository.GetAllAsync(cancellationToken);
        return empresas.Select(e => new EmpresaDto
        {
            Id = e.Id,
            Name = e.Name,
            Nit = e.Nit,
            Email = e.Email,
            Phone = e.Phone,
            Address = e.Address,
            IsActive = e.IsActive
        });
    }

    public async Task<EmpresaDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var e = await _repository.GetByIdAsync(id, cancellationToken);
        if (e == null) return null;

        return new EmpresaDto
        {
            Id = e.Id,
            Name = e.Name,
            Nit = e.Nit,
            Email = e.Email,
            Phone = e.Phone,
            Address = e.Address,
            IsActive = e.IsActive
        };
    }

    public async Task<EmpresaDto> CreateAsync(SaveEmpresaDto dto, CancellationToken cancellationToken = default)
    {
        var empresa = new Empresa
        {
            Name = dto.Name,
            Nit = dto.Nit,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address
        };

        var created = await _repository.AddAsync(empresa, cancellationToken);

        return new EmpresaDto
        {
            Id = created.Id,
            Name = created.Name,
            Nit = created.Nit,
            Email = created.Email,
            Phone = created.Phone,
            Address = created.Address,
            IsActive = created.IsActive
        };
    }

    public async Task<bool> UpdateAsync(Guid id, SaveEmpresaDto dto, CancellationToken cancellationToken = default)
    {
        var empresa = await _repository.GetByIdAsync(id, cancellationToken);
        if (empresa == null) return false;

        empresa.Name = dto.Name;
        empresa.Nit = dto.Nit;
        empresa.Email = dto.Email;
        empresa.Phone = dto.Phone;
        empresa.Address = dto.Address;

        await _repository.UpdateAsync(empresa, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var empresa = await _repository.GetByIdAsync(id, cancellationToken);
        if (empresa == null) return false;

        await _repository.DeleteAsync(id, cancellationToken);
        return true;
    }
}
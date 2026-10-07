using Firmeza.Application.DTOs.Clientes;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IRepository<Cliente> _repository;

    public ClienteService(IRepository<Cliente> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ClienteDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clientes = await _repository.GetAllAsync(cancellationToken);
        return clientes.Select(c => new ClienteDto
        {
            Id = c.Id,
            Name = c.Name,
            DocumentNumber = c.DocumentNumber,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address,
            UserId = c.UserId,
            IsActive = c.IsActive
        });
    }

    public async Task<ClienteDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var c = await _repository.GetByIdAsync(id, cancellationToken);
        if (c == null) return null;

        return new ClienteDto
        {
            Id = c.Id,
            Name = c.Name,
            DocumentNumber = c.DocumentNumber,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address,
            UserId = c.UserId,
            IsActive = c.IsActive
        };
    }

    public async Task<ClienteDto> CreateAsync(SaveClienteDto dto, CancellationToken cancellationToken = default)
    {
        var cliente = new Cliente
        {
            Name = dto.Name,
            DocumentNumber = dto.DocumentNumber,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address
        };

        var created = await _repository.AddAsync(cliente, cancellationToken);

        return new ClienteDto
        {
            Id = created.Id,
            Name = created.Name,
            DocumentNumber = created.DocumentNumber,
            Email = created.Email,
            Phone = created.Phone,
            Address = created.Address,
            IsActive = created.IsActive
        };
    }

    public async Task<bool> UpdateAsync(Guid id, SaveClienteDto dto, CancellationToken cancellationToken = default)
    {
        var cliente = await _repository.GetByIdAsync(id, cancellationToken);
        if (cliente == null) return false;

        cliente.Name = dto.Name;
        cliente.DocumentNumber = dto.DocumentNumber;
        cliente.Email = dto.Email;
        cliente.Phone = dto.Phone;
        cliente.Address = dto.Address;

        await _repository.UpdateAsync(cliente, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await _repository.GetByIdAsync(id, cancellationToken);
        if (cliente == null) return false;

        await _repository.DeleteAsync(id, cancellationToken);
        return true;
    }
}
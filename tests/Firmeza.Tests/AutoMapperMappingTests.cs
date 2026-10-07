using AutoMapper;
using Firmeza.Application.DTOs.Clientes;
using Firmeza.Application.DTOs.Empresas;
using Firmeza.Application.Mappings;
using Firmeza.Domain.Entities;
using Xunit;

namespace Firmeza.Tests;

/// <summary>
/// Pruebas unitarias para validar la configuración y perfiles de AutoMapper para Cliente y Empresa.
/// </summary>
public class AutoMapperMappingTests
{
    private readonly IMapper _mapper;

    public AutoMapperMappingTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ClienteMappingProfile>();
            cfg.AddProfile<EmpresaMappingProfile>();
        });

        _mapper = config.CreateMapper();
    }

    [Fact]
    public void ClienteMapping_ShouldMapClienteToClienteDtoWithAge()
    {
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente
        {
            Id = clienteId,
            Name = "Carlos Gomez",
            DocumentNumber = "12345678",
            Email = "carlos@gmail.com",
            BirthDate = DateTime.UtcNow.AddYears(-30)
        };

        var dto = _mapper.Map<ClienteDto>(cliente);

        Assert.Equal(cliente.Id, dto.Id);
        Assert.Equal(cliente.Name, dto.Name);
        Assert.Equal(30, dto.Age);
        Assert.True(dto.IsActive);
    }

    [Fact]
    public void EmpresaMapping_ShouldMapCompanyProperties()
    {
        var empresaId = Guid.NewGuid();
        var empresa = new Empresa
        {
            Id = empresaId,
            Name = "Distribuidora Central",
            Nit = "901234567-2",
            Email = "info@central.example",
            Phone = "3001234567",
            Address = "Calle 100 # 20-30"
        };

        var dto = _mapper.Map<EmpresaDto>(empresa);

        Assert.Equal(empresa.Id, dto.Id);
        Assert.Equal(empresa.Name, dto.Name);
        Assert.Equal(empresa.Nit, dto.Nit);
        Assert.Equal(empresa.Email, dto.Email);
        Assert.True(dto.IsActive);
    }
}
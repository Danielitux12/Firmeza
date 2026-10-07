using AutoMapper;
using Firmeza.Application.DTOs.Clientes;
using Firmeza.Application.DTOs.Empresas;
using Firmeza.Application.DTOs.Products;
using Firmeza.Application.DTOs.Sales;
using Firmeza.Application.Mappings;
using Firmeza.Domain.Entities;
using Firmeza.Domain.Enums;
using Xunit;

namespace Firmeza.Tests;

/// <summary>
/// Pruebas unitarias para validar la configuración y perfiles de AutoMapper.
/// </summary>
public class AutoMapperMappingTests
{
    private readonly IMapper _mapper;

    public AutoMapperMappingTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductMappingProfile>();
            cfg.AddProfile<ClienteMappingProfile>();
            cfg.AddProfile<EmpresaMappingProfile>();
            cfg.AddProfile<SaleMappingProfile>();
        });

        _mapper = config.CreateMapper();
    }

    [Fact]
    public void ProductMapping_ShouldMapProductToProductDto()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Martillo",
            Description = "Martillo de acero",
            Price = 25000,
            Stock = 10,
            Category = "Herramientas",
            Status = ProductStatus.Available
        };

        var dto = _mapper.Map<ProductDto>(product);

        Assert.Equal(product.Id, dto.Id);
        Assert.Equal(product.Name, dto.Name);
        Assert.Equal(product.Price, dto.Price);
    }

    [Fact]
    public void ClienteMapping_ShouldMapClienteToClienteDtoWithAge()
    {
        var cliente = new Cliente
        {
            Id = 1,
            Name = "Carlos Gomez",
            DocumentNumber = "12345678",
            Email = "carlos@gmail.com",
            BirthDate = DateTime.Today.AddYears(-30)
        };

        var dto = _mapper.Map<ClienteDto>(cliente);

        Assert.Equal(cliente.Id, dto.Id);
        Assert.Equal(cliente.Name, dto.Name);
        Assert.Equal(30, dto.Age);
    }

    [Fact]
    public void EmpresaMapping_ShouldMapCompanyProperties()
    {
        var empresa = new Empresa
        {
            Id = 7,
            Name = "Distribuidora Central",
            Nit = "901234567-2",
            Email = "info@central.example"
        };

        var dto = _mapper.Map<EmpresaDto>(empresa);

        Assert.Equal(empresa.Id, dto.Id);
        Assert.Equal(empresa.Name, dto.Name);
        Assert.Equal(empresa.Nit, dto.Nit);
        Assert.Equal(empresa.IsActive, dto.IsActive);
    }

    [Fact]
    public void SaleMapping_ShouldMapSaleToSaleDtoWithDetails()
    {
        var product = new Product { Id = 1, Name = "Taladro" };
        var sale = new Sale
        {
            Id = 5,
            SaleNumber = "VTA-001",
            Date = DateTime.UtcNow,
            Cliente = new Cliente { Name = "Laura Lopez" },
            Subtotal = 100,
            Tax = 19,
            Total = 119
        };
        sale.SaleDetails.Add(new SaleDetail
        {
            Id = 10,
            ProductId = 1,
            Product = product,
            Quantity = 2,
            UnitPrice = 50,
            LineTotal = 100
        });

        var dto = _mapper.Map<SaleDto>(sale);

        Assert.Equal("VTA-001", dto.SaleNumber);
        Assert.Equal("Laura Lopez", dto.ClienteName);
        Assert.Single(dto.Details);
        Assert.Equal("Taladro", dto.Details[0].ProductName);
    }
}

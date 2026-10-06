using AutoMapper;
using Firmeza.Application.DTOs.Customers;
using Firmeza.Application.DTOs.Products;
using Firmeza.Application.DTOs.Sales;
using Firmeza.Application.Mappings;
using Firmeza.Domain.Entities;
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
            cfg.AddProfile<CustomerMappingProfile>();
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
            IsAvailable = true
        };

        var dto = _mapper.Map<ProductDto>(product);

        Assert.Equal(product.Id, dto.Id);
        Assert.Equal(product.Name, dto.Name);
        Assert.Equal(product.Price, dto.Price);
    }

    [Fact]
    public void CustomerMapping_ShouldMapCustomerToCustomerDtoWithAge()
    {
        var customer = new Customer
        {
            Id = 1,
            FullName = "Carlos Gomez",
            DocumentNumber = "12345678",
            Email = "carlos@gmail.com",
            BirthDate = DateTime.Today.AddYears(-30)
        };

        var dto = _mapper.Map<CustomerDto>(customer);

        Assert.Equal(customer.Id, dto.Id);
        Assert.Equal(customer.FullName, dto.FullName);
        Assert.Equal(30, dto.Age);
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
            Customer = new Customer { FullName = "Laura Lopez" },
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
        Assert.Equal("Laura Lopez", dto.CustomerName);
        Assert.Single(dto.Details);
        Assert.Equal("Taladro", dto.Details[0].ProductName);
    }
}

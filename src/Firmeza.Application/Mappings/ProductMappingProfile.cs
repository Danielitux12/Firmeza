using AutoMapper;
using Firmeza.Application.DTOs.Products;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Mappings;

/// <summary>
/// Perfil de mapeo simple de AutoMapper para la entidad Producto.
/// Permite transformar entre la entidad de dominio y los DTOs de entrada/salida.
/// </summary>
public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // De Entidad a DTO de lectura
        CreateMap<Product, ProductDto>();

        // De DTO de creación/edición a Entidad
        CreateMap<SaveProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.SaleDetails, opt => opt.Ignore());
    }
}

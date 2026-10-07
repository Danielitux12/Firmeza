using AutoMapper;
using Firmeza.Application.DTOs.Sales;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Mappings;

/// <summary>
/// Perfil de mapeo simple de AutoMapper para Ventas y Detalles de Venta.
/// </summary>
public class SaleMappingProfile : Profile
{
    public SaleMappingProfile()
    {
        // De detalle de venta a DTO
        CreateMap<SaleDetail, SaleDetailDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

        // De venta a DTO
        CreateMap<Sale, SaleDto>()
            .ForMember(dest => dest.ClienteName, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Name : string.Empty))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.SaleDetails));
    }
}

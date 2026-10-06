using AutoMapper;
using Firmeza.Application.DTOs.Customers;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Mappings;

/// <summary>
/// Perfil de mapeo simple de AutoMapper para la entidad Cliente.
/// </summary>
public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        // De Entidad a DTO de lectura
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.BirthDate.HasValue ? DateTime.Today.Year - src.BirthDate.Value.Year : 0));

        // De DTO de guardado a Entidad
        CreateMap<SaveCustomerDto, Customer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Sales, opt => opt.Ignore())
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.Age > 0 ? DateTime.UtcNow.AddYears(-src.Age) : (DateTime?)null));
    }
}

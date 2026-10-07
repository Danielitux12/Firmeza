using AutoMapper;
using Firmeza.Application.DTOs.Clientes;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Mappings;

/// <summary>
/// Perfil de mapeo simple de AutoMapper para la entidad Cliente.
/// </summary>
public class ClienteMappingProfile : Profile
{
    public ClienteMappingProfile()
    {
        // De Entidad a DTO de lectura
        CreateMap<Cliente, ClienteDto>()
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.BirthDate.HasValue ? DateTime.Today.Year - src.BirthDate.Value.Year : 0));

        // De DTO de guardado a Entidad
        CreateMap<SaveClienteDto, Cliente>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Sales, opt => opt.Ignore())
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.Age > 0 ? DateTime.UtcNow.AddYears(-src.Age) : (DateTime?)null));
    }
}

using AutoMapper;
using Firmeza.Application.DTOs.Trabajadores;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Mappings;

public class TrabajadorMappingProfile : Profile
{
    public TrabajadorMappingProfile()
    {
        CreateMap<Trabajador, TrabajadorDto>();

        CreateMap<SaveTrabajadorDto, Trabajador>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());
    }
}
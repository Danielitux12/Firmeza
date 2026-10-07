using AutoMapper;
using Firmeza.Application.DTOs.Empresas;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Mappings;

public class EmpresaMappingProfile : Profile
{
    public EmpresaMappingProfile()
    {
        CreateMap<Empresa, EmpresaDto>();

        CreateMap<SaveEmpresaDto, Empresa>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore());
    }
}
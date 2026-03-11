using AutoMapper;
using FluxoCaixa.Application.DTOs;
using FluxoCaixa.Domain.Entities;

namespace FluxoCaixa.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Lancamento, LancamentoDto>()
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => (int)src.Tipo))
            .ForMember(dest => dest.TipoDescricao, opt => opt.MapFrom(src => src.Tipo.ToString()));

        CreateMap<ConsolidadoDiario, ConsolidadoDiarioDto>();
    }
}

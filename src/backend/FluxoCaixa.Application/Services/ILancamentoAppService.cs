using FluxoCaixa.Application.DTOs;

namespace FluxoCaixa.Application.Services;

public interface ILancamentoAppService
{
    Task<LancamentoDto> CreateAsync(CreateLancamentoDto dto);
    Task<LancamentoDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<LancamentoDto>> GetAllAsync();
}

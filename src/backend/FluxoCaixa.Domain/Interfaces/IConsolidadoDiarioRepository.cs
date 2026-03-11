using FluxoCaixa.Domain.Entities;

namespace FluxoCaixa.Domain.Interfaces;

public interface IConsolidadoDiarioRepository
{
    Task<ConsolidadoDiario?> GetByDataAsync(DateTime data);
    Task AddAsync(ConsolidadoDiario consolidado);
    Task UpdateAsync(ConsolidadoDiario consolidado);
    Task SaveChangesAsync();
}

using FluxoCaixa.Domain.Entities;

namespace FluxoCaixa.Domain.Interfaces;

public interface ILancamentoRepository
{
    Task<Lancamento?> GetByIdAsync(Guid id);
    Task<IEnumerable<Lancamento>> GetAllAsync();
    Task<IEnumerable<Lancamento>> GetByDataAsync(DateTime data);
    Task AddAsync(Lancamento lancamento);
    Task SaveChangesAsync();
}

using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces;
using FluxoCaixa.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FluxoCaixa.Infrastructure.Repositories;

public class LancamentoRepository : ILancamentoRepository
{
    private readonly FluxoCaixaDbContext _context;

    public LancamentoRepository(FluxoCaixaDbContext context)
    {
        _context = context;
    }

    public async Task<Lancamento?> GetByIdAsync(Guid id)
    {
        return await _context.Lancamentos.FindAsync(id);
    }

    public async Task<IEnumerable<Lancamento>> GetAllAsync()
    {
        return await _context.Lancamentos
            .OrderByDescending(l => l.DataLancamento)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lancamento>> GetByDataAsync(DateTime data)
    {
        return await _context.Lancamentos
            .Where(l => l.DataLancamento.Date == data.Date)
            .ToListAsync();
    }

    public async Task AddAsync(Lancamento lancamento)
    {
        await _context.Lancamentos.AddAsync(lancamento);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces;
using FluxoCaixa.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FluxoCaixa.Infrastructure.Repositories;

public class ConsolidadoDiarioRepository : IConsolidadoDiarioRepository
{
    private readonly FluxoCaixaDbContext _context;

    public ConsolidadoDiarioRepository(FluxoCaixaDbContext context)
    {
        _context = context;
    }

    public async Task<ConsolidadoDiario?> GetByDataAsync(DateTime data)
    {
        return await _context.ConsolidadosDiarios
            .FirstOrDefaultAsync(c => c.Data.Date == data.Date);
    }

    public async Task AddAsync(ConsolidadoDiario consolidado)
    {
        await _context.ConsolidadosDiarios.AddAsync(consolidado);
    }

    public async Task UpdateAsync(ConsolidadoDiario consolidado)
    {
        _context.ConsolidadosDiarios.Update(consolidado);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

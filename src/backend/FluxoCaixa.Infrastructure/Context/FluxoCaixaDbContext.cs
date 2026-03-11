using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Infrastructure.Mappings;
using Microsoft.EntityFrameworkCore;

namespace FluxoCaixa.Infrastructure.Context;

public class FluxoCaixaDbContext : DbContext
{
    public DbSet<Lancamento> Lancamentos { get; set; } = null!;
    public DbSet<ConsolidadoDiario> ConsolidadosDiarios { get; set; } = null!;

    public FluxoCaixaDbContext(DbContextOptions<FluxoCaixaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new LancamentoMap());
        modelBuilder.ApplyConfiguration(new ConsolidadoDiarioMap());
        base.OnModelCreating(modelBuilder);
    }
}

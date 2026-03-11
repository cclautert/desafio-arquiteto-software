using FluxoCaixa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluxoCaixa.Infrastructure.Mappings;

public class ConsolidadoDiarioMap : IEntityTypeConfiguration<ConsolidadoDiario>
{
    public void Configure(EntityTypeBuilder<ConsolidadoDiario> builder)
    {
        builder.ToTable("ConsolidadosDiarios");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Data)
            .IsRequired();

        builder.Property(c => c.TotalCreditos)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.TotalDebitos)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Ignore(c => c.Saldo);

        builder.Property(c => c.UpdatedAt)
            .IsRequired();
    }
}

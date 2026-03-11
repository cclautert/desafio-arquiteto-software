using FluxoCaixa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluxoCaixa.Infrastructure.Mappings;

public class LancamentoMap : IEntityTypeConfiguration<Lancamento>
{
    public void Configure(EntityTypeBuilder<Lancamento> builder)
    {
        builder.ToTable("Lancamentos");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Descricao)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.Valor)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(l => l.Tipo)
            .IsRequired();

        builder.Property(l => l.DataLancamento)
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .IsRequired();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RRHH.Domain.Entidades;

namespace RRHH.Infrastructure.Datos.Configuraciones;

public class ColaboradorConfiguracion : IEntityTypeConfiguration<Colaborador>
{
    public void Configure(EntityTypeBuilder<Colaborador> b)
    {
        b.ToTable("Colaborador");
        b.HasKey(c => c.Id).HasName("PK_Colaborador");
        b.Property(c => c.NombreCompleto).HasMaxLength(200).IsRequired();
        b.Property(c => c.Telefono).HasMaxLength(20).IsRequired();
        b.Property(c => c.Correo).HasMaxLength(254).IsRequired();
        b.HasIndex(c => c.Correo).IsUnique().HasDatabaseName("UQ_Colaborador_Correo");
    }
}

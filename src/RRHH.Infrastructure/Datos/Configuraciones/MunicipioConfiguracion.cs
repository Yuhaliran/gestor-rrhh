using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RRHH.Domain.Entidades;

namespace RRHH.Infrastructure.Datos.Configuraciones;

public class MunicipioConfiguracion : IEntityTypeConfiguration<Municipio>
{
    public void Configure(EntityTypeBuilder<Municipio> b)
    {
        b.ToTable("Municipio");
        b.HasKey(m => m.Id).HasName("PK_Municipio");
        b.Property(m => m.Nombre).HasMaxLength(100).IsRequired();
        b.HasIndex(m => new { m.DepartamentoId, m.Nombre }).IsUnique().HasDatabaseName("UQ_Municipio_DepartamentoId_Nombre");

        // RN1
        b.HasOne(m => m.Departamento).WithMany(d => d.Municipios)
         .HasForeignKey(m => m.DepartamentoId)
         .OnDelete(DeleteBehavior.Restrict)
         .HasConstraintName("FK_Municipio_Departamento");
    }
}

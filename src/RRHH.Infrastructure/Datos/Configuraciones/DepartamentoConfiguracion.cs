using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RRHH.Domain.Entidades;

namespace RRHH.Infrastructure.Datos.Configuraciones;

public class DepartamentoConfiguracion : IEntityTypeConfiguration<Departamento>
{
    public void Configure(EntityTypeBuilder<Departamento> b)
    {
        b.ToTable("Departamento");
        b.HasKey(d => d.Id).HasName("PK_Departamento");
        b.Property(d => d.Nombre).HasMaxLength(100).IsRequired();
        b.HasIndex(d => new { d.PaisId, d.Nombre }).IsUnique().HasDatabaseName("UQ_Departamento_PaisId_Nombre");

        // Nunca borrado en cascada en la geografía (RN1)
        b.HasOne(d => d.Pais).WithMany(p => p.Departamentos)
         .HasForeignKey(d => d.PaisId)
         .OnDelete(DeleteBehavior.Restrict)
         .HasConstraintName("FK_Departamento_Pais");

        b.HasData(DatosIniciales.Departamentos());
    }
}

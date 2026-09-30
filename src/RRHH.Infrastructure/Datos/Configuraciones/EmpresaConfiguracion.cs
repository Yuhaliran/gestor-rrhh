using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RRHH.Domain.Entidades;

namespace RRHH.Infrastructure.Datos.Configuraciones;

public class EmpresaConfiguracion : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> b)
    {
        b.ToTable("Empresa");
        b.HasKey(e => e.Id).HasName("PK_Empresa");
        b.Property(e => e.Nit).HasMaxLength(20).IsRequired();
        b.Property(e => e.RazonSocial).HasMaxLength(200).IsRequired();
        b.Property(e => e.NombreComercial).HasMaxLength(200).IsRequired();
        b.Property(e => e.Telefono).HasMaxLength(20).IsRequired();
        b.Property(e => e.Correo).HasMaxLength(254).IsRequired();

        // No único: el NIT es único dentro de cada país y eso lo valida el servicio (RN5)
        b.HasIndex(e => e.Nit).HasDatabaseName("IX_Empresa_Nit");

        // RN1
        b.HasOne(e => e.Municipio).WithMany(m => m.Empresas)
         .HasForeignKey(e => e.MunicipioId)
         .OnDelete(DeleteBehavior.Restrict)
         .HasConstraintName("FK_Empresa_Municipio");
    }
}

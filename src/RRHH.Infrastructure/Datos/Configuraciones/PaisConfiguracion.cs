using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RRHH.Domain.Entidades;

namespace RRHH.Infrastructure.Datos.Configuraciones;

public class PaisConfiguracion : IEntityTypeConfiguration<Pais>
{
    public void Configure(EntityTypeBuilder<Pais> b)
    {
        // Invariante del rango de edad (RN4); los valores por defecto están en el dominio, no aquí.
        b.ToTable("Pais", t => t.HasCheckConstraint("CK_Pais_RangoEdad", "EdadMinima >= 0 AND EdadMinima <= EdadMaxima"));
        b.HasKey(p => p.Id).HasName("PK_Pais");
        b.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
        b.Property(p => p.CodigoIso2).HasMaxLength(2).IsFixedLength().IsRequired();
        b.Property(p => p.Regla29Febrero).HasConversion<string>().HasMaxLength(20);   // legible en la base
        b.HasIndex(p => p.Nombre).IsUnique().HasDatabaseName("UQ_Pais_Nombre");
        b.HasIndex(p => p.CodigoIso2).IsUnique().HasDatabaseName("UQ_Pais_CodigoIso2");

        b.HasData(DatosIniciales.Paises());
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RRHH.Domain.Entidades;

namespace RRHH.Infrastructure.Datos.Configuraciones;

public class EmpresaColaboradorConfiguracion : IEntityTypeConfiguration<EmpresaColaborador>
{
    public void Configure(EntityTypeBuilder<EmpresaColaborador> b)
    {
        // Contrato: sólo la clave compuesta, sin la cual EF no puede armar el modelo.
        // El resto de las configuraciones se agregan después de las pruebas del tester.
        b.HasKey(ec => new { ec.EmpresaId, ec.ColaboradorId });
    }
}

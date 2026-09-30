using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RRHH.Domain.Entidades;

namespace RRHH.Infrastructure.Datos.Configuraciones;

public class EmpresaColaboradorConfiguracion : IEntityTypeConfiguration<EmpresaColaborador>
{
    public void Configure(EntityTypeBuilder<EmpresaColaborador> b)
    {
        b.ToTable("EmpresaColaborador");
        b.HasKey(ec => new { ec.EmpresaId, ec.ColaboradorId }).HasName("PK_EmpresaColaborador");
        b.Property(ec => ec.Puesto).HasMaxLength(100);                   // único dato opcional

        // No se borra una empresa con colaboradores (RN2)
        b.HasOne(ec => ec.Empresa).WithMany(e => e.Colaboradores)
         .HasForeignKey(ec => ec.EmpresaId)
         .OnDelete(DeleteBehavior.Restrict)
         .HasConstraintName("FK_EmpresaColaborador_Empresa");

        // Al borrar un colaborador se borran sus relaciones con empresas
        b.HasOne(ec => ec.Colaborador).WithMany(c => c.Empresas)
         .HasForeignKey(ec => ec.ColaboradorId)
         .OnDelete(DeleteBehavior.Cascade)
         .HasConstraintName("FK_EmpresaColaborador_Colaborador");
    }
}

using Microsoft.EntityFrameworkCore;

using RRHH.Domain.Entidades;

namespace RRHH.Application.Datos;

// Acceso a datos de los servicios. Lo implementa RrhhDbContext (Infrastructure); los servicios
// dependen de esta interfaz y no de Infrastructure (PLAN.md, «Arquitectura»).
public interface IRrhhDbContext
{
    DbSet<Pais> Paises { get; }
    DbSet<Departamento> Departamentos { get; }
    DbSet<Municipio> Municipios { get; }
    DbSet<Empresa> Empresas { get; }
    DbSet<Colaborador> Colaboradores { get; }
    DbSet<EmpresaColaborador> EmpresasColaboradores { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}

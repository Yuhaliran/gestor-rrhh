using Microsoft.EntityFrameworkCore;

using RRHH.Application.Datos;
using RRHH.Domain.Entidades;

namespace RRHH.Infrastructure.Datos;

public class RrhhDbContext(DbContextOptions<RrhhDbContext> opciones) : DbContext(opciones), IRrhhDbContext
{
    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<Municipio> Municipios => Set<Municipio>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Colaborador> Colaboradores => Set<Colaborador>();
    public DbSet<EmpresaColaborador> EmpresasColaboradores => Set<EmpresaColaborador>();

    // Una clase <Entidad>Configuracion por entidad, en Datos/Configuraciones (CODIFICACION.md).
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RrhhDbContext).Assembly);
}

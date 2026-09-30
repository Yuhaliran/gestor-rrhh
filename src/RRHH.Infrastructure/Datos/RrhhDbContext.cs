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
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RrhhDbContext).Assembly);

        // Los textos se comparan sin distinguir mayúsculas y sí tildes (RN5), en la base: así las
        // consultas y los índices únicos lo respetan sin UPPER(), que impide usar los índices.
        // SQL Server: intercalación española (ordena la Ñ). SQLite (pruebas): NOCASE, sólo ASCII.
        // EF guarda el modelo por tipo de contexto: un mismo proceso usa un solo motor.
        var intercalacion = Database.IsSqlServer() ? "Modern_Spanish_CI_AS" : "NOCASE";
        var textos = modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.ClrType == typeof(string));
        foreach (var texto in textos)
        {
            texto.SetCollation(intercalacion);
        }
    }
}

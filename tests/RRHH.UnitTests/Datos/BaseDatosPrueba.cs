using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RRHH.Infrastructure.Datos;

namespace RRHH.UnitTests.Datos;

public sealed class BaseDatosPrueba : IDisposable
{
    private readonly SqliteConnection _conexion = new("DataSource=:memory:");
    public RrhhDbContext Contexto { get; }

    public BaseDatosPrueba()
    {
        _conexion.Open();
        var opciones = new DbContextOptionsBuilder<RrhhDbContext>().UseSqlite(_conexion).Options;
        Contexto = new RrhhDbContext(opciones);
        Contexto.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Contexto.Dispose();
        _conexion.Dispose();
    }
}

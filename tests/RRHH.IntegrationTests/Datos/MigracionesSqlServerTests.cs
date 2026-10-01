using Microsoft.EntityFrameworkCore;

using RRHH.Domain.Reglas;
using RRHH.Infrastructure.Datos;

namespace RRHH.IntegrationTests.Datos;

public class MigracionesSqlServerTests : IAsyncLifetime
{
    private readonly string _bdNombre = $"Rrhh_Mig2_{Guid.NewGuid()}";
    private readonly string _servidor;
    private readonly RrhhDbContext _contexto;

    public MigracionesSqlServerTests()
    {
        _servidor = Environment.GetEnvironmentVariable("RRHH_MIG2_SERVIDOR") ?? @"(localdb)\MSSQLLocalDB";
        var conexion = $"Server={_servidor};Database={_bdNombre};Trusted_Connection=True;TrustServerCertificate=True";

        var opciones = new DbContextOptionsBuilder<RrhhDbContext>()
            .UseSqlServer(conexion)
            .Options;

        _contexto = new RrhhDbContext(opciones);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _contexto.Database.EnsureDeletedAsync();
        await _contexto.DisposeAsync();
    }

    [Fact]
    [Trait("Categoria", "SqlServer")]
    public async Task Migrar_BaseVacia_CreaTablasYDatosIniciales()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var departamentosEsperados = new[]
        {
            (1, "Guatemala", "Guatemala"),
            (2, "El Progreso", "Guastatoya"),
            (3, "Sacatepéquez", "Antigua Guatemala"),
            (4, "Chimaltenango", "Chimaltenango"),
            (5, "Escuintla", "Escuintla"),
            (6, "Santa Rosa", "Cuilapa"),
            (7, "Sololá", "Sololá"),
            (8, "Totonicapán", "Totonicapán"),
            (9, "Quetzaltenango", "Quetzaltenango"),
            (10, "Suchitepéquez", "Mazatenango"),
            (11, "Retalhuleu", "Retalhuleu"),
            (12, "San Marcos", "San Marcos"),
            (13, "Huehuetenango", "Huehuetenango"),
            (14, "Quiché", "Santa Cruz del Quiché"),
            (15, "Baja Verapaz", "Salamá"),
            (16, "Alta Verapaz", "Cobán"),
            (17, "Petén", "Flores"),
            (18, "Izabal", "Puerto Barrios"),
            (19, "Zacapa", "Zacapa"),
            (20, "Chiquimula", "Chiquimula"),
            (21, "Jalapa", "Jalapa"),
            (22, "Jutiapa", "Jutiapa")
        };

        // Act
        await _contexto.Database.MigrateAsync(ct);

        // Assert
        var pendientes = await _contexto.Database.GetPendingMigrationsAsync(ct);
        Assert.Empty(pendientes);

        var aplicadas = await _contexto.Database.GetAppliedMigrationsAsync(ct);
        var delProyecto = _contexto.Database.GetMigrations();
        Assert.Equal(delProyecto, aplicadas);

        var paises = await _contexto.Paises.ToListAsync(ct);
        var departamentos = await _contexto.Departamentos.ToListAsync(ct);
        var municipios = await _contexto.Municipios.ToListAsync(ct);
        var empresas = await _contexto.Empresas.ToListAsync(ct);
        var colaboradores = await _contexto.Colaboradores.ToListAsync(ct);
        var empresasColaboradores = await _contexto.EmpresasColaboradores.ToListAsync(ct);

        var guate = Assert.Single(paises);
        Assert.Equal(1, guate.Id);
        Assert.Equal("Guatemala", guate.Nombre);
        Assert.Equal("GT", guate.CodigoIso2);
        Assert.Equal(18, guate.EdadMinima);
        Assert.Equal(100, guate.EdadMaxima);
        Assert.Equal(Regla29Febrero.VeintiochoDeFebrero, guate.Regla29Febrero);

        Assert.Equal(22, departamentos.Count);
        Assert.Equal(22, municipios.Count);

        foreach (var (id, deptoNombre, cabeceraNombre) in departamentosEsperados)
        {
            var depto = departamentos.SingleOrDefault(d => d.Id == id);
            Assert.NotNull(depto);
            Assert.Equal(guate.Id, depto.PaisId);
            Assert.Equal(deptoNombre, depto.Nombre);

            var muni = municipios.SingleOrDefault(m => m.Id == id);
            Assert.NotNull(muni);
            Assert.Equal(depto.Id, muni.DepartamentoId);
            Assert.Equal(cabeceraNombre, muni.Nombre);
        }

        Assert.Empty(empresas);
        Assert.Empty(colaboradores);
        Assert.Empty(empresasColaboradores);

        await _contexto.Database.OpenConnectionAsync(ct);
        using var cmd = _contexto.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = @"
            SELECT collation_name 
            FROM sys.columns 
            WHERE object_id = OBJECT_ID('Pais') AND name = 'Nombre'";

        var collation = (string)(await cmd.ExecuteScalarAsync(ct) ?? string.Empty);
        Assert.Equal("Modern_Spanish_CI_AS", collation);
    }
}

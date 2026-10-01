using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RRHH.Infrastructure.Datos;
using RRHH.Domain.Reglas;

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
        var ct = TestContext.Current.CancellationToken;

        // 1. Aplicar migraciones
        await _contexto.Database.MigrateAsync(ct);

        // 2. Verificar que no quedan migraciones pendientes
        var pendientes = await _contexto.Database.GetPendingMigrationsAsync(ct);
        Assert.Empty(pendientes);

        // Verificar que las aplicadas son las del proyecto
        var aplicadas = await _contexto.Database.GetAppliedMigrationsAsync(ct);
        Assert.NotEmpty(aplicadas);

        // 3. Verificar que cada tabla existe (consultar cada DbSet)
        var paises = await _contexto.Paises.ToListAsync(ct);
        var departamentos = await _contexto.Departamentos.ToListAsync(ct);
        var municipios = await _contexto.Municipios.ToListAsync(ct);
        var empresas = await _contexto.Empresas.ToListAsync(ct);
        var colaboradores = await _contexto.Colaboradores.ToListAsync(ct);
        var empresasColaboradores = await _contexto.EmpresasColaboradores.ToListAsync(ct);

        // 4. Datos iniciales de PLAN.md
        // Guatemala con id 1, GT, edades 18 a 100 y regla del 28 de febrero
        var guate = Assert.Single(paises);
        Assert.Equal(1, guate.Id);
        Assert.Equal("Guatemala", guate.Nombre);
        Assert.Equal("GT", guate.CodigoIso2);
        Assert.Equal(18, guate.EdadMinima);
        Assert.Equal(100, guate.EdadMaxima);
        Assert.Equal(Regla29Febrero.VeintiochoDeFebrero, guate.Regla29Febrero);

        // Los 22 departamentos y sus 22 cabeceras con su id
        Assert.Equal(22, departamentos.Count);
        Assert.Equal(22, municipios.Count);

        foreach (var depto in departamentos)
        {
            var muni = municipios.SingleOrDefault(m => m.Id == depto.Id);
            Assert.NotNull(muni);
            Assert.Equal(depto.Id, muni.DepartamentoId);
        }

        // Ninguna empresa ni colaborador
        Assert.Empty(empresas);
        Assert.Empty(colaboradores);
        Assert.Empty(empresasColaboradores);

        // 5. La intercalación de los textos es Modern_Spanish_CI_AS
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

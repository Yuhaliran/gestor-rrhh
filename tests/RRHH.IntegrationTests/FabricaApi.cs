using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using RRHH.Infrastructure.Datos;

namespace RRHH.IntegrationTests;

public class FabricaApi : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _conexion = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _conexion.Open();
        builder.ConfigureServices(services =>
        {
            // Quitar la configuración de SQL Server
            services.RemoveAll<DbContextOptions<RrhhDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<RrhhDbContext>>();
            services.AddDbContext<RrhhDbContext>(o => o.UseSqlite(_conexion));

            // Reemplazar TimeProvider
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(new RelojFijo());

            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<RrhhDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _conexion.Dispose();
    }

    private class RelojFijo : TimeProvider
    {
        private readonly DateTimeOffset _ahora = new(2027, 2, 28, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => _ahora;
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}

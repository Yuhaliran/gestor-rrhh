using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using RRHH.Application.Datos;
using RRHH.Infrastructure.Datos;

namespace RRHH.Infrastructure;

public static class ServiciosInfraestructura
{
    // La cadena de conexión viene de user-secrets de RRHH.Api, nunca del repositorio (PLAN.md).
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection servicios, string? cadenaConexion)
    {
        servicios.AddDbContext<RrhhDbContext>(o => o.UseSqlServer(cadenaConexion));
        servicios.AddScoped<IRrhhDbContext>(proveedor => proveedor.GetRequiredService<RrhhDbContext>());

        // /health verifica también que se llega a la base (PLAN.md, «API»)
        servicios.AddHealthChecks().AddDbContextCheck<RrhhDbContext>("base-de-datos");
        return servicios;
    }
}

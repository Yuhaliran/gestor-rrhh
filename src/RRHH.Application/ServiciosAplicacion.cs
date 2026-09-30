using Microsoft.Extensions.DependencyInjection;

using RRHH.Application.Interfaces;
using RRHH.Application.Servicios;

namespace RRHH.Application;

public static class ServiciosAplicacion
{
    public static IServiceCollection AgregarAplicacion(this IServiceCollection servicios)
    {
        servicios.AddScoped<IPaisesServicio, PaisesServicio>();
        servicios.AddScoped<IDepartamentosServicio, DepartamentosServicio>();
        return servicios;
    }
}

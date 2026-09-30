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
        servicios.AddScoped<IMunicipiosServicio, MunicipiosServicio>();
        servicios.AddScoped<IEmpresasServicio, EmpresasServicio>();
        servicios.AddScoped<IColaboradoresServicio, ColaboradoresServicio>();

        // Fecha actual de los servicios (edad, RN4, RN9); las pruebas usan un reloj fijo
        servicios.AddSingleton(TimeProvider.System);
        return servicios;
    }
}

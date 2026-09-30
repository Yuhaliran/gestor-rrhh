using RRHH.Application.Datos;
using RRHH.Application.Interfaces;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Paises;

namespace RRHH.Application.Servicios;

// Contrato de la tarea 9: los métodos se implementan después de las pruebas del tester.
public class PaisesServicio(IRrhhDbContext db) : IPaisesServicio
{
    public Task<Pagina<PaisDto>> ListarAsync(Consulta consulta, CancellationToken ct) => throw Pendiente();

    public Task<PaisDto> ObtenerAsync(int id, CancellationToken ct) => throw Pendiente();

    public Task<PaisDto> CrearAsync(GuardarPaisDto dto, CancellationToken ct) => throw Pendiente();

    public Task<PaisDto> ActualizarAsync(int id, GuardarPaisDto dto, CancellationToken ct) => throw Pendiente();

    public Task EliminarAsync(int id, CancellationToken ct) => throw Pendiente();

    private NotImplementedException Pendiente()
    {
        _ = db;
        return new NotImplementedException("Contrato de la tarea 9: se implementa después de las pruebas.");
    }
}

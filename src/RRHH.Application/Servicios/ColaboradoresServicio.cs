using RRHH.Application.Datos;
using RRHH.Application.Interfaces;
using RRHH.Contratos.Colaboradores;
using RRHH.Contratos.Comun;

namespace RRHH.Application.Servicios;

// Contrato de la tarea 13: los métodos se implementan después de las pruebas del tester.
public class ColaboradoresServicio(IRrhhDbContext db, TimeProvider reloj) : IColaboradoresServicio
{
    public Task<Pagina<ColaboradorDto>> ListarAsync(Consulta consulta, CancellationToken ct) => throw Pendiente();

    public Task<ColaboradorDto> ObtenerAsync(int id, CancellationToken ct) => throw Pendiente();

    public Task<ColaboradorDto> CrearAsync(CrearColaboradorDto dto, CancellationToken ct) => throw Pendiente();

    public Task<ColaboradorDto> ActualizarAsync(int id, GuardarColaboradorDto dto, CancellationToken ct) => throw Pendiente();

    public Task EliminarAsync(int id, CancellationToken ct) => throw Pendiente();

    private NotImplementedException Pendiente()
    {
        _ = db;
        _ = reloj;
        return new NotImplementedException("Contrato de la tarea 13: se implementa después de las pruebas.");
    }
}

using RRHH.Application.Datos;
using RRHH.Application.Interfaces;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Departamentos;

namespace RRHH.Application.Servicios;

// Contrato de la tarea 10: los métodos se implementan después de las pruebas del tester.
public class DepartamentosServicio(IRrhhDbContext db) : IDepartamentosServicio
{
    public Task<Pagina<DepartamentoDto>> ListarAsync(Consulta consulta, CancellationToken ct) => throw Pendiente();

    public Task<IReadOnlyList<DepartamentoDto>> ListarPorPaisAsync(int paisId, CancellationToken ct) => throw Pendiente();

    public Task<DepartamentoDto> ObtenerAsync(int id, CancellationToken ct) => throw Pendiente();

    public Task<DepartamentoDto> CrearAsync(GuardarDepartamentoDto dto, CancellationToken ct) => throw Pendiente();

    public Task<DepartamentoDto> ActualizarAsync(int id, GuardarDepartamentoDto dto, CancellationToken ct) => throw Pendiente();

    public Task EliminarAsync(int id, CancellationToken ct) => throw Pendiente();

    private NotImplementedException Pendiente()
    {
        _ = db;
        return new NotImplementedException("Contrato de la tarea 10: se implementa después de las pruebas.");
    }
}

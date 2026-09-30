using RRHH.Application.Datos;
using RRHH.Application.Interfaces;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Municipios;

namespace RRHH.Application.Servicios;

// Contrato de la tarea 11: los métodos se implementan después de las pruebas del tester.
public class MunicipiosServicio(IRrhhDbContext db) : IMunicipiosServicio
{
    public Task<Pagina<MunicipioDto>> ListarAsync(Consulta consulta, CancellationToken ct) => throw Pendiente();

    public Task<IReadOnlyList<MunicipioDto>> ListarPorDepartamentoAsync(int departamentoId, CancellationToken ct) => throw Pendiente();

    public Task<MunicipioDto> ObtenerAsync(int id, CancellationToken ct) => throw Pendiente();

    public Task<MunicipioDto> CrearAsync(GuardarMunicipioDto dto, CancellationToken ct) => throw Pendiente();

    public Task<MunicipioDto> ActualizarAsync(int id, GuardarMunicipioDto dto, CancellationToken ct) => throw Pendiente();

    public Task EliminarAsync(int id, CancellationToken ct) => throw Pendiente();

    private NotImplementedException Pendiente()
    {
        _ = db;
        return new NotImplementedException("Contrato de la tarea 11: se implementa después de las pruebas.");
    }
}

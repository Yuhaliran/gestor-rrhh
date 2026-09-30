using RRHH.Application.Datos;
using RRHH.Application.Interfaces;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Empresas;

namespace RRHH.Application.Servicios;

// Contrato de la tarea 12: los métodos se implementan después de las pruebas del tester.
public class EmpresasServicio(IRrhhDbContext db) : IEmpresasServicio
{
    public Task<Pagina<EmpresaDto>> ListarAsync(Consulta consulta, CancellationToken ct) => throw Pendiente();

    public Task<EmpresaDto> ObtenerAsync(int id, CancellationToken ct) => throw Pendiente();

    public Task<EmpresaDto> CrearAsync(GuardarEmpresaDto dto, CancellationToken ct) => throw Pendiente();

    public Task<EmpresaDto> ActualizarAsync(int id, GuardarEmpresaDto dto, CancellationToken ct) => throw Pendiente();

    public Task EliminarAsync(int id, CancellationToken ct) => throw Pendiente();

    private NotImplementedException Pendiente()
    {
        _ = db;
        return new NotImplementedException("Contrato de la tarea 12: se implementa después de las pruebas.");
    }
}

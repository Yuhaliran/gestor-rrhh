using RRHH.Contratos.Comun;
using RRHH.Contratos.Departamentos;

namespace RRHH.Application.Interfaces;

// Mantenimiento de departamentos. El nombre es único dentro del país (RN5): no distingue
// mayúsculas ni espacios en los extremos, y sí tildes.
public interface IDepartamentosServicio
{
    // Ordenado por nombre del país y después por nombre. Buscar filtra por el nombre del
    // departamento, sin distinguir mayúsculas.
    Task<Pagina<DepartamentoDto>> ListarAsync(Consulta consulta, CancellationToken ct);

    // Todos los departamentos del país, sin paginar y ordenados por nombre (listas en cascada, RN6).
    // NoEncontradoException (404) si el país no existe; lista vacía si no tiene departamentos.
    Task<IReadOnlyList<DepartamentoDto>> ListarPorPaisAsync(int paisId, CancellationToken ct);

    // NoEncontradoException (404) si no existe.
    Task<DepartamentoDto> ObtenerAsync(int id, CancellationToken ct);

    // ValidacionException (400) en el campo PaisId si el país no existe (V4); ConflictoException
    // (409) si el nombre ya existe en el país (RN5). Guarda el nombre sin espacios en los extremos.
    Task<DepartamentoDto> CrearAsync(GuardarDepartamentoDto dto, CancellationToken ct);

    // Mismos errores que CrearAsync, después de NoEncontradoException (404) si el departamento no
    // existe: se revisan en ese orden (404, 400, 409). Puede cambiar el país; el nombre se valida en
    // el país nuevo, y el propio departamento no cuenta como duplicado.
    Task<DepartamentoDto> ActualizarAsync(int id, GuardarDepartamentoDto dto, CancellationToken ct);

    // NoEncontradoException (404); ConflictoException (409) si tiene municipios (RN1).
    Task EliminarAsync(int id, CancellationToken ct);
}

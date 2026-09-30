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

    // Se revisa en este orden: NoEncontradoException (404) si el departamento no existe;
    // ValidacionException (400) en el campo PaisId si es distinto del actual (RN8: el país no cambia
    // al editar); ConflictoException (409) si el nombre ya existe en el país (RN5), sin contar al
    // propio departamento. Guarda el nombre sin espacios en los extremos.
    Task<DepartamentoDto> ActualizarAsync(int id, GuardarDepartamentoDto dto, CancellationToken ct);

    // NoEncontradoException (404); ConflictoException (409) si tiene municipios (RN1).
    Task EliminarAsync(int id, CancellationToken ct);
}

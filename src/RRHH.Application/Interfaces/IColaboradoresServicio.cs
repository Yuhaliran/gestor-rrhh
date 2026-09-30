using RRHH.Contratos.Colaboradores;
using RRHH.Contratos.Comun;

namespace RRHH.Application.Interfaces;

// Mantenimiento de colaboradores. La fecha actual (edad, RN4, RN9) la da el TimeProvider del
// servicio. La edad que se muestra usa la regla del 29 de febrero del país de la empresa con la
// fecha de ingreso más antigua; si hay varias, la de menor id (RN7).
public interface IColaboradoresServicio
{
    // Ordenado por nombre completo y después por id. Buscar filtra por nombre o correo, sin
    // distinguir mayúsculas.
    Task<Pagina<ColaboradorDto>> ListarAsync(Consulta consulta, CancellationToken ct);

    // NoEncontradoException (404) si no existe.
    Task<ColaboradorDto> ObtenerAsync(int id, CancellationToken ct);

    // Primero los 400 (ValidacionException), en este orden:
    //   "Empresas[i].EmpresaId" si la empresa no existe (V4);
    //   "FechaNacimiento" si la edad de hoy está fuera del rango del país de alguna empresa (RN4;
    //   una fecha de nacimiento futura da una edad negativa, RN7);
    //   "Empresas[i].FechaIngreso" si es posterior a hoy o anterior a la fecha de nacimiento (RN9).
    // Después los 409 (ConflictoException): una empresa repetida en la lista, o el correo de otro
    // colaborador (RN5). Guarda los textos sin espacios en los extremos.
    Task<ColaboradorDto> CrearAsync(CrearColaboradorDto dto, CancellationToken ct);

    // NoEncontradoException (404) si no existe. Si cambia la fecha de nacimiento, revalida contra sus
    // empresas: RN4 y RN9, los dos con ValidacionException (400) en "FechaNacimiento". Después,
    // ConflictoException (409) si el correo es de otro colaborador (RN5). Sus empresas no cambian.
    Task<ColaboradorDto> ActualizarAsync(int id, GuardarColaboradorDto dto, CancellationToken ct);

    // NoEncontradoException (404) si no existe. Borra también sus relaciones con empresas.
    Task EliminarAsync(int id, CancellationToken ct);

    // Colaboradores de una empresa, paginado, con el mismo orden y búsqueda que ListarAsync.
    // NoEncontradoException (404) si la empresa no existe.
    Task<Pagina<ColaboradorDto>> ListarPorEmpresaAsync(int empresaId, Consulta consulta, CancellationToken ct);

    // Asocia el colaborador a otra empresa y lo devuelve actualizado. Se revisa en este orden:
    // NoEncontradoException (404) si el colaborador no existe; ValidacionException (400) en
    // "EmpresaId" si la empresa no existe (V4) o si la edad de hoy está fuera del rango de su país
    // (RN4, sólo contra la empresa nueva); en "FechaIngreso" si es posterior a hoy o anterior al
    // nacimiento (RN9); ConflictoException (409) si ya estaba asociado a esa empresa (RN5).
    Task<ColaboradorDto> AsociarEmpresaAsync(int id, AsociarEmpresaDto dto, CancellationToken ct);

    // Edita la fecha de ingreso y el puesto de una relación y devuelve el colaborador actualizado.
    // NoEncontradoException (404) si el colaborador no existe o no está asociado a la empresa;
    // ValidacionException (400) en "FechaIngreso" (RN9).
    Task<ColaboradorDto> ActualizarEmpresaAsync(int id, int empresaId, GuardarEmpresaColaboradorDto dto, CancellationToken ct);

    // Quita una empresa del colaborador. NoEncontradoException (404) si el colaborador no existe o
    // no está asociado a la empresa; ConflictoException (409) si es la última (RN3).
    Task QuitarEmpresaAsync(int id, int empresaId, CancellationToken ct);
}

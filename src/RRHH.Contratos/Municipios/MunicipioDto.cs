namespace RRHH.Contratos.Municipios;

// Incluye el departamento y el país para mostrarlos en los listados y en el formulario de edición
// (donde no se pueden cambiar, RN8) sin otra consulta.
public record MunicipioDto(int Id, string Nombre, int DepartamentoId, string DepartamentoNombre, int PaisId, string PaisNombre);

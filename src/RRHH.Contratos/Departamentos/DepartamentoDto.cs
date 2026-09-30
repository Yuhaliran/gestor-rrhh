namespace RRHH.Contratos.Departamentos;

// Incluye el nombre del país para mostrarlo en los listados sin otra consulta.
public record DepartamentoDto(int Id, string Nombre, int PaisId, string PaisNombre);

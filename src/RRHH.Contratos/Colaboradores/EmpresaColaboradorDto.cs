namespace RRHH.Contratos.Colaboradores;

// Una empresa del colaborador, con los datos de la relación.
public record EmpresaColaboradorDto(
    int EmpresaId,
    string NombreComercial,
    int PaisId,
    string PaisNombre,
    DateOnly FechaIngreso,
    string? Puesto);

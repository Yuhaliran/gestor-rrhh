namespace RRHH.Contratos.Colaboradores;

// Listado y detalle: con la edad calculada (RN7) y sus empresas (CA2).
public record ColaboradorDto(
    int Id,
    string NombreCompleto,
    DateOnly FechaNacimiento,
    int Edad,
    string Telefono,
    string Correo,
    IReadOnlyList<EmpresaColaboradorDto> Empresas);

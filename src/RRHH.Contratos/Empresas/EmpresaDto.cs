namespace RRHH.Contratos.Empresas;

// Detalle y listado, con la geografía completa (CA1).
public record EmpresaDto(
    int Id,
    string Nit,
    string RazonSocial,
    string NombreComercial,
    string Telefono,
    string Correo,
    int MunicipioId,
    string MunicipioNombre,
    int DepartamentoId,
    string DepartamentoNombre,
    int PaisId,
    string PaisNombre);

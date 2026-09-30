using System.ComponentModel.DataAnnotations;

using RRHH.Contratos.Comun;

namespace RRHH.Contratos.Empresas;

// Datos para crear o editar una empresa. Que el municipio exista (V4) y que no sea de otro país al
// editar (RN8), y que el NIT no se repita en el país (RN5), lo valida el servicio.
public record GuardarEmpresaDto
{
    [Required(ErrorMessage = "El municipio es obligatorio.")]
    public int? MunicipioId { get; init; }

    [Required(ErrorMessage = "El NIT es obligatorio.")]
    [StringLength(20, ErrorMessage = "El NIT admite hasta 20 caracteres.")]
    public string? Nit { get; init; }

    [Required(ErrorMessage = "La razón social es obligatoria.")]
    [StringLength(200, ErrorMessage = "La razón social admite hasta 200 caracteres.")]
    public string? RazonSocial { get; init; }

    [Required(ErrorMessage = "El nombre comercial es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre comercial admite hasta 200 caracteres.")]
    public string? NombreComercial { get; init; }

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(Formatos.Telefono,
        ErrorMessage = "El teléfono admite dígitos, espacios, +, - y paréntesis, de 7 a 20 caracteres.")]
    public string? Telefono { get; init; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(254, ErrorMessage = "El correo admite hasta 254 caracteres.")]
    public string? Correo { get; init; }
}

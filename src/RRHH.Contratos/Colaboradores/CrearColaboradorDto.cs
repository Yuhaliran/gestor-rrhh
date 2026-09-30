using System.ComponentModel.DataAnnotations;

namespace RRHH.Contratos.Colaboradores;

// Alta de un colaborador: sus datos personales y sus empresas, al menos una (RN3).
// La API valida también cada elemento de Empresas; Validator.TryValidateObject no entra en la lista.
public record CrearColaboradorDto : GuardarColaboradorDto
{
    [Required(ErrorMessage = "Las empresas son obligatorias.")]
    [MinLength(1, ErrorMessage = "El colaborador tiene que tener al menos una empresa.")]
    public IReadOnlyList<AsociarEmpresaDto>? Empresas { get; init; }
}

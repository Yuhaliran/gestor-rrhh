namespace RRHH.Domain.Entidades;

public class Colaborador
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = "";
    public DateOnly FechaNacimiento { get; set; }               // la edad no se guarda: se calcula (RN7)
    public string Telefono { get; set; } = "";
    public string Correo { get; set; } = "";
    public ICollection<EmpresaColaborador> Empresas { get; set; } = [];
}

namespace RRHH.Domain.Entidades;

public class Empresa
{
    public int Id { get; set; }
    public int MunicipioId { get; set; }                        // departamento y país salen del municipio
    public string Nit { get; set; } = "";
    public string RazonSocial { get; set; } = "";
    public string NombreComercial { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Correo { get; set; } = "";
    public Municipio Municipio { get; set; } = null!;
    public ICollection<EmpresaColaborador> Colaboradores { get; set; } = [];
}

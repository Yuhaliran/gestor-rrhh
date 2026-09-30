namespace RRHH.Domain.Entidades;

public class Municipio
{
    public int Id { get; set; }
    public int DepartamentoId { get; set; }
    public string Nombre { get; set; } = "";
    public Departamento Departamento { get; set; } = null!;
    public ICollection<Empresa> Empresas { get; set; } = [];
}

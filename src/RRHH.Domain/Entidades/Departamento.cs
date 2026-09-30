namespace RRHH.Domain.Entidades;

public class Departamento
{
    public int Id { get; set; }
    public int PaisId { get; set; }
    public string Nombre { get; set; } = "";
    public Pais Pais { get; set; } = null!;
    public ICollection<Municipio> Municipios { get; set; } = [];
}

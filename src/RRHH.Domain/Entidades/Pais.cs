using RRHH.Domain.Reglas;

namespace RRHH.Domain.Entidades;

public class Pais
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string CodigoIso2 { get; set; } = "";
    public int EdadMinima { get; set; } = 18;                  // legislación de cada país (RN4)
    public int EdadMaxima { get; set; } = 100;
    public Regla29Febrero Regla29Febrero { get; set; } = Regla29Febrero.VeintiochoDeFebrero;   // RN7
    public ICollection<Departamento> Departamentos { get; set; } = [];
}

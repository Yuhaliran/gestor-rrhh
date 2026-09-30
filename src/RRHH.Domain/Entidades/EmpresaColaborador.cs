namespace RRHH.Domain.Entidades;

public class EmpresaColaborador
{
    public int EmpresaId { get; set; }
    public int ColaboradorId { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public string? Puesto { get; set; }                         // único dato opcional
    public Empresa Empresa { get; set; } = null!;
    public Colaborador Colaborador { get; set; } = null!;
}

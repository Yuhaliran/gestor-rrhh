namespace RRHH.Contratos.Paises;

public record PaisDto(int Id, string Nombre, string CodigoIso2, int EdadMinima, int EdadMaxima, Regla29Febrero Regla29Febrero);

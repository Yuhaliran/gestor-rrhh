namespace RRHH.Contratos.Comun;

// Una página de un listado: sus elementos, el total de la consulta y la página pedida.
public record Pagina<T>(IReadOnlyList<T> Elementos, int Total, int Numero, int Tamanio)
{
    public int TotalPaginas => (Total + Tamanio - 1) / Tamanio;   // para la paginación de la web
}

using RRHH.Contratos.Comun;

namespace RRHH.Web.Pages.Shared;

// Datos del paginador (_Paginacion): la página actual, el total y el texto buscado, que se conserva
// al cambiar de página.
public record Paginacion(int Numero, int TotalPaginas, int Total, string? Buscar)
{
    public static Paginacion De<T>(Pagina<T> pagina, string? buscar) =>
        new(pagina.Numero, pagina.TotalPaginas, pagina.Total, buscar);
}

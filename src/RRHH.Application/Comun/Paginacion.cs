using RRHH.Contratos.Comun;

namespace RRHH.Application.Comun;

public static class Paginacion
{
    // Cuenta el total de la consulta y trae la página pedida; la consulta tiene que venir ordenada.
    // Una página posterior a la última devuelve la lista vacía, con el total real.
    // Contrato de la tarea 8: todavía devuelve una página vacía; se implementa después de las pruebas.
    public static Task<Pagina<T>> PaginarAsync<T>(this IQueryable<T> consulta, Consulta c, CancellationToken ct) =>
        Task.FromResult(new Pagina<T>([], 0, c.Pagina, c.Tamanio));
}

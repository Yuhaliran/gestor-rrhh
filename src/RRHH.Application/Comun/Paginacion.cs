using Microsoft.EntityFrameworkCore;

using RRHH.Contratos.Comun;

namespace RRHH.Application.Comun;

public static class Paginacion
{
    // Cuenta el total de la consulta y trae la página pedida; la consulta tiene que venir ordenada.
    // Una página posterior a la última devuelve la lista vacía, con el total real.
    public static async Task<Pagina<T>> PaginarAsync<T>(this IQueryable<T> consulta, Consulta c, CancellationToken ct)
    {
        var total = await consulta.CountAsync(ct);

        // En long: una página muy grande multiplicada por el tamaño no entra en un int
        var saltear = (long)(c.Pagina - 1) * c.Tamanio;
        List<T> elementos = saltear >= total
            ? []
            : await consulta.Skip((int)saltear).Take(c.Tamanio).ToListAsync(ct);

        return new Pagina<T>(elementos, total, c.Pagina, c.Tamanio);
    }
}

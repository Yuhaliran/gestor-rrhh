namespace RRHH.Application.Comun;

public static class Busqueda
{
    public const string Escape = "\\";

    // Patrón de LIKE para «contiene»: escapa los comodines del texto buscado (%, _, [ y el propio
    // escape) para que se busquen literalmente. Se usa con EF.Functions.Like(columna, patrón, Escape);
    // la intercalación de la columna hace que no distinga mayúsculas.
    public static string PatronContiene(string texto)
    {
        var escapado = texto.Trim()
            .Replace(Escape, Escape + Escape)
            .Replace("%", Escape + "%")
            .Replace("_", Escape + "_")
            .Replace("[", Escape + "[");
        return $"%{escapado}%";
    }
}

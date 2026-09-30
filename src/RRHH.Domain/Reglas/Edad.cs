namespace RRHH.Domain.Reglas;

public static class Edad
{
    // Años cumplidos a la fecha `hoy` (RN7). Los nacidos el 29 de febrero cumplen, en años no
    // bisiestos, el día que indica `regla`. Negativa si la fecha de nacimiento es posterior a hoy.
    // La validación del rango de edad (RN4) se aplica en el servicio de colaboradores.
    public static int Calcular(DateOnly fechaNacimiento, DateOnly hoy, Regla29Febrero regla)
    {
        var edad = hoy.Year - fechaNacimiento.Year;
        if (hoy < CumpleaniosEn(hoy.Year, fechaNacimiento, regla)) edad--;   // todavía no cumplió este año
        return edad;
    }

    private static DateOnly CumpleaniosEn(int anio, DateOnly fechaNacimiento, Regla29Febrero regla)
    {
        if (fechaNacimiento is not { Month: 2, Day: 29 } || DateTime.IsLeapYear(anio))
            return new DateOnly(anio, fechaNacimiento.Month, fechaNacimiento.Day);

        return regla switch
        {
            Regla29Febrero.VeintiochoDeFebrero => new DateOnly(anio, 2, 28),
            Regla29Febrero.PrimeroDeMarzo => new DateOnly(anio, 3, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(regla), regla, "Regla del 29 de febrero desconocida.")
        };
    }
}

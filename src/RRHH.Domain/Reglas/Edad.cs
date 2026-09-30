namespace RRHH.Domain.Reglas;

public static class Edad
{
    // Años cumplidos a la fecha `hoy` (RN7). Los nacidos el 29 de febrero cumplen, en años no
    // bisiestos, el día que indica `regla`. Negativa si la fecha de nacimiento es posterior a hoy.
    // La validación del rango de edad (RN4) se aplica en el servicio de colaboradores.
    public static int Calcular(DateOnly fechaNacimiento, DateOnly hoy, Regla29Febrero regla) =>
        throw new NotImplementedException("Contrato de la tarea 4: se implementa después de las pruebas.");
}

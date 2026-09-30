namespace RRHH.Application.Excepciones;

// Una regla del servicio sobre un campo (V4, RN4): la API responde 400 con el error en ese campo,
// con el mismo formato que los errores de validación del DTO.
public class ValidacionException(string campo, string mensaje) : Exception(mensaje)
{
    public string Campo { get; } = campo;
}

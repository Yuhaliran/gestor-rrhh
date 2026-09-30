namespace RRHH.Application.Excepciones;

// Una regla de negocio impide la operación (RN1, RN2, RN3, RN5): la API responde 409.
public class ConflictoException(string mensaje) : Exception(mensaje);

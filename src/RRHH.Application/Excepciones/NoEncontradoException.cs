namespace RRHH.Application.Excepciones;

// El recurso pedido no existe: la API responde 404.
public class NoEncontradoException(string recurso, int id) : Exception($"No existe {recurso} con id {id}.");

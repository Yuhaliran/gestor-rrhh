using Microsoft.AspNetCore.Mvc;

using RRHH.Application.Interfaces;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Departamentos;

namespace RRHH.Api.Controllers;

// Delgado: la lógica está en el servicio; los errores los traduce ManejadorExcepciones.
[ApiController]
[Route("api/departamentos")]
public class DepartamentosController(IDepartamentosServicio servicio) : ControllerBase
{
    [HttpGet]
    public Task<Pagina<DepartamentoDto>> Listar([FromQuery] Consulta consulta, CancellationToken ct) =>
        servicio.ListarAsync(consulta, ct);

    // Listas en cascada (RN6). Ruta absoluta: cuelga del país, pero es de este servicio.
    [HttpGet("/api/paises/{paisId:int}/departamentos")]
    public Task<IReadOnlyList<DepartamentoDto>> ListarPorPais(int paisId, CancellationToken ct) =>
        servicio.ListarPorPaisAsync(paisId, ct);

    [HttpGet("{id:int}")]
    public Task<DepartamentoDto> Obtener(int id, CancellationToken ct) => servicio.ObtenerAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<DepartamentoDto>> Crear(GuardarDepartamentoDto dto, CancellationToken ct)
    {
        var creado = await servicio.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);   // 201 + Location
    }

    [HttpPut("{id:int}")]
    public Task<DepartamentoDto> Actualizar(int id, GuardarDepartamentoDto dto, CancellationToken ct) =>
        servicio.ActualizarAsync(id, dto, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await servicio.EliminarAsync(id, ct);
        return NoContent();                                                           // 204
    }
}

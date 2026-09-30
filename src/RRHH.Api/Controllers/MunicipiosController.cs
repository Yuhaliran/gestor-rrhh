using Microsoft.AspNetCore.Mvc;

using RRHH.Application.Interfaces;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Municipios;

namespace RRHH.Api.Controllers;

// Delgado: la lógica está en el servicio; los errores los traduce ManejadorExcepciones.
[ApiController]
[Route("api/municipios")]
public class MunicipiosController(IMunicipiosServicio servicio) : ControllerBase
{
    [HttpGet]
    public Task<Pagina<MunicipioDto>> Listar([FromQuery] Consulta consulta, CancellationToken ct) =>
        servicio.ListarAsync(consulta, ct);

    // Listas en cascada (RN6). Ruta absoluta: cuelga del departamento, pero es de este servicio.
    [HttpGet("/api/departamentos/{departamentoId:int}/municipios")]
    public Task<IReadOnlyList<MunicipioDto>> ListarPorDepartamento(int departamentoId, CancellationToken ct) =>
        servicio.ListarPorDepartamentoAsync(departamentoId, ct);

    [HttpGet("{id:int}")]
    public Task<MunicipioDto> Obtener(int id, CancellationToken ct) => servicio.ObtenerAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<MunicipioDto>> Crear(GuardarMunicipioDto dto, CancellationToken ct)
    {
        var creado = await servicio.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);   // 201 + Location
    }

    [HttpPut("{id:int}")]
    public Task<MunicipioDto> Actualizar(int id, GuardarMunicipioDto dto, CancellationToken ct) =>
        servicio.ActualizarAsync(id, dto, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await servicio.EliminarAsync(id, ct);
        return NoContent();                                                           // 204
    }
}

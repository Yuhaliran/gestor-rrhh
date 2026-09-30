using Microsoft.AspNetCore.Mvc;

using RRHH.Application.Interfaces;
using RRHH.Contratos.Colaboradores;
using RRHH.Contratos.Comun;

namespace RRHH.Api.Controllers;

// Delgado: la lógica está en el servicio; los errores los traduce ManejadorExcepciones.
[ApiController]
[Route("api/colaboradores")]
public class ColaboradoresController(IColaboradoresServicio servicio) : ControllerBase
{
    [HttpGet]
    public Task<Pagina<ColaboradorDto>> Listar([FromQuery] Consulta consulta, CancellationToken ct) =>
        servicio.ListarAsync(consulta, ct);

    [HttpGet("{id:int}")]
    public Task<ColaboradorDto> Obtener(int id, CancellationToken ct) => servicio.ObtenerAsync(id, ct);

    // Con sus empresas, al menos una (RN3)
    [HttpPost]
    public async Task<ActionResult<ColaboradorDto>> Crear(CrearColaboradorDto dto, CancellationToken ct)
    {
        var creado = await servicio.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);   // 201 + Location
    }

    // Sólo los datos personales: las empresas se manejan con /api/colaboradores/{id}/empresas
    [HttpPut("{id:int}")]
    public Task<ColaboradorDto> Actualizar(int id, GuardarColaboradorDto dto, CancellationToken ct) =>
        servicio.ActualizarAsync(id, dto, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await servicio.EliminarAsync(id, ct);
        return NoContent();                                                           // 204
    }
}

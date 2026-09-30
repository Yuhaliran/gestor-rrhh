using Microsoft.AspNetCore.Mvc;

using RRHH.Application.Interfaces;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Empresas;

namespace RRHH.Api.Controllers;

// Delgado: la lógica está en el servicio; los errores los traduce ManejadorExcepciones.
[ApiController]
[Route("api/empresas")]
public class EmpresasController(IEmpresasServicio servicio) : ControllerBase
{
    [HttpGet]
    public Task<Pagina<EmpresaDto>> Listar([FromQuery] Consulta consulta, CancellationToken ct) =>
        servicio.ListarAsync(consulta, ct);

    [HttpGet("{id:int}")]
    public Task<EmpresaDto> Obtener(int id, CancellationToken ct) => servicio.ObtenerAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<EmpresaDto>> Crear(GuardarEmpresaDto dto, CancellationToken ct)
    {
        var creada = await servicio.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, creada);   // 201 + Location
    }

    [HttpPut("{id:int}")]
    public Task<EmpresaDto> Actualizar(int id, GuardarEmpresaDto dto, CancellationToken ct) =>
        servicio.ActualizarAsync(id, dto, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await servicio.EliminarAsync(id, ct);
        return NoContent();                                                           // 204
    }
}

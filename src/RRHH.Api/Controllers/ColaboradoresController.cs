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

    // Colaboradores de una empresa. Ruta absoluta: cuelga de la empresa, pero es de este servicio.
    [HttpGet("/api/empresas/{empresaId:int}/colaboradores")]
    public Task<Pagina<ColaboradorDto>> ListarPorEmpresa(int empresaId, [FromQuery] Consulta consulta, CancellationToken ct) =>
        servicio.ListarPorEmpresaAsync(empresaId, consulta, ct);

    // Empresas del colaborador: devuelven el colaborador actualizado (con su edad), no hay un GET de
    // la relación al que apunte un Location
    [HttpPost("{id:int}/empresas")]
    public Task<ColaboradorDto> AsociarEmpresa(int id, AsociarEmpresaDto dto, CancellationToken ct) =>
        servicio.AsociarEmpresaAsync(id, dto, ct);

    [HttpPut("{id:int}/empresas/{empresaId:int}")]
    public Task<ColaboradorDto> ActualizarEmpresa(int id, int empresaId, GuardarEmpresaColaboradorDto dto, CancellationToken ct) =>
        servicio.ActualizarEmpresaAsync(id, empresaId, dto, ct);

    [HttpDelete("{id:int}/empresas/{empresaId:int}")]
    public async Task<IActionResult> QuitarEmpresa(int id, int empresaId, CancellationToken ct)
    {
        await servicio.QuitarEmpresaAsync(id, empresaId, ct);
        return NoContent();                                                           // 204
    }
}

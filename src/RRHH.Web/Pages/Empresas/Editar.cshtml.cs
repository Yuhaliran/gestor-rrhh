using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RRHH.Contratos.Departamentos;
using RRHH.Contratos.Empresas;
using RRHH.Contratos.Municipios;
using RRHH.Contratos.Paises;
using RRHH.Web.Api;

namespace RRHH.Web.Pages.Empresas;

// Alta (sin id) y edición (con id) de una empresa. La geografía se elige en cascada: país,
// departamento y municipio (RN6). Al editar, el país no cambia: la empresa sólo se muda dentro de
// su país (RN8).
public class EditarModel(ClienteRrhh api) : PageModel
{
    [BindProperty]
    public GuardarEmpresaDto Datos { get; set; } = new();

    // Sólo para la cascada: la empresa guarda el municipio
    [BindProperty]
    public int? PaisId { get; set; }

    [BindProperty]
    public int? DepartamentoId { get; set; }

    public int? Id { get; private set; }
    public string? PaisNombre { get; private set; }

    public IReadOnlyList<PaisDto> Paises { get; private set; } = [];
    public IReadOnlyList<DepartamentoDto> Departamentos { get; private set; } = [];
    public IReadOnlyList<MunicipioDto> Municipios { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int? id, CancellationToken ct)
    {
        if (id is not null)
        {
            var empresa = await api.ObtenerEmpresaAsync(id.Value, ct);
            if (empresa is null)
            {
                return NotFound();
            }
            Datos = new GuardarEmpresaDto
            {
                MunicipioId = empresa.MunicipioId,
                Nit = empresa.Nit,
                RazonSocial = empresa.RazonSocial,
                NombreComercial = empresa.NombreComercial,
                Telefono = empresa.Telefono,
                Correo = empresa.Correo
            };
            PaisId = empresa.PaisId;
            DepartamentoId = empresa.DepartamentoId;
        }
        return await MostrarAsync(id, ct);
    }

    public async Task<IActionResult> OnPostAsync(int? id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return await MostrarAsync(id, ct);
        }
        var resultado = id is null
            ? await api.CrearEmpresaAsync(Datos, ct)
            : await api.ActualizarEmpresaAsync(id.Value, Datos, ct);
        if (!resultado.Exitoso)
        {
            ModelState.AgregarProblema(resultado.Problema!, nameof(Datos));
            return await MostrarAsync(id, ct);
        }
        TempData["Mensaje"] = id is null ? "Empresa creada." : "Empresa actualizada.";
        return RedirectToPage("Index");
    }

    // Las opciones de cada nivel según lo elegido en el anterior
    private async Task<IActionResult> MostrarAsync(int? id, CancellationToken ct)
    {
        Id = id;
        if (id is null)
        {
            Paises = await api.TodosLosPaisesAsync(ct);
        }
        else if (PaisId is not null)
        {
            PaisNombre = (await api.ObtenerPaisAsync(PaisId.Value, ct))?.Nombre;
        }
        if (PaisId is not null)
        {
            Departamentos = await api.DepartamentosDePaisAsync(PaisId.Value, ct);
        }
        if (DepartamentoId is not null)
        {
            Municipios = await api.MunicipiosDeDepartamentoAsync(DepartamentoId.Value, ct);
        }
        return Page();
    }
}

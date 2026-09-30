using RRHH.Domain.Entidades;

namespace RRHH.Infrastructure.Datos;

// Datos iniciales de Guatemala (PLAN.md, «Datos iniciales»). Fuente: codificación nacional de
// departamentos y municipios del INE. Los ids de departamento son su código del INE, y cada
// cabecera (código INE DD01) tiene el mismo id que su departamento.
internal static class DatosIniciales
{
    private const int IdGuatemala = 1;

    private static readonly (int Id, string Departamento, string Cabecera)[] Guatemala =
    [
        (1, "Guatemala", "Guatemala"),
        (2, "El Progreso", "Guastatoya"),
        (3, "Sacatepéquez", "Antigua Guatemala"),
        (4, "Chimaltenango", "Chimaltenango"),
        (5, "Escuintla", "Escuintla"),
        (6, "Santa Rosa", "Cuilapa"),
        (7, "Sololá", "Sololá"),
        (8, "Totonicapán", "Totonicapán"),
        (9, "Quetzaltenango", "Quetzaltenango"),
        (10, "Suchitepéquez", "Mazatenango"),
        (11, "Retalhuleu", "Retalhuleu"),
        (12, "San Marcos", "San Marcos"),
        (13, "Huehuetenango", "Huehuetenango"),
        (14, "Quiché", "Santa Cruz del Quiché"),
        (15, "Baja Verapaz", "Salamá"),
        (16, "Alta Verapaz", "Cobán"),
        (17, "Petén", "Flores"),
        (18, "Izabal", "Puerto Barrios"),
        (19, "Zacapa", "Zacapa"),
        (20, "Chiquimula", "Chiquimula"),
        (21, "Jalapa", "Jalapa"),
        (22, "Jutiapa", "Jutiapa"),
    ];

    // Edad mínima, máxima y regla del 29 de febrero: los valores por defecto del dominio.
    public static Pais[] Paises() => [new Pais { Id = IdGuatemala, Nombre = "Guatemala", CodigoIso2 = "GT" }];

    public static Departamento[] Departamentos() =>
        [.. Guatemala.Select(d => new Departamento { Id = d.Id, PaisId = IdGuatemala, Nombre = d.Departamento })];

    public static Municipio[] Municipios() =>
        [.. Guatemala.Select(d => new Municipio { Id = d.Id, DepartamentoId = d.Id, Nombre = d.Cabecera })];
}

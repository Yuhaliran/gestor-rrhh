namespace RRHH.Contratos.Paises;

// Día en que cumplen años los nacidos el 29 de febrero, en los años no bisiestos (RN7).
// Repite el enum del dominio porque Contratos no depende de nadie (ARQ1); el servicio los traduce.
public enum Regla29Febrero
{
    VeintiochoDeFebrero,
    PrimeroDeMarzo
}

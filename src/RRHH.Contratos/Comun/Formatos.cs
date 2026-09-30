namespace RRHH.Contratos.Comun;

// Formatos de validación que comparten varios DTOs.
public static class Formatos
{
    // V5: dígitos, espacios, +, - y paréntesis, de 7 a 20 caracteres
    public const string Telefono = @"^[0-9+()\- ]{7,20}$";
}

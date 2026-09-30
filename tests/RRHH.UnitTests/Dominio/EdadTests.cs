namespace RRHH.UnitTests.Dominio;

using System.Globalization;

using RRHH.Domain.Reglas;

public class EdadTests
{
    [Theory]
    [InlineData("2000-05-15", "2026-05-15", 26)] // Cumpleaños hoy
    [InlineData("2000-05-15", "2026-05-14", 25)] // Cumpleaños mañana (falta 1 día)
    [InlineData("2000-05-15", "2026-05-16", 26)] // Cumpleaños ayer (pasó 1 día)
    public void Calcular_CumpleaniosHoyManianaYAyer_DevuelveEdadSegunSiYaCumplio(
        string fechaNacimientoTexto, string hoyTexto, int edadEsperada)
    {
        // Arrange
        var fechaNacimiento = Fecha(fechaNacimientoTexto);
        var hoy = Fecha(hoyTexto);

        // Act
        var edad = Edad.Calcular(fechaNacimiento, hoy, Regla29Febrero.VeintiochoDeFebrero);

        // Assert
        Assert.Equal(edadEsperada, edad);
    }

    [Theory]
    [InlineData("2025-02-27", 24)] // Día anterior: aún no cumple
    [InlineData("2025-02-28", 25)] // Cumpleaños hoy según regla 28 de febrero
    [InlineData("2025-03-01", 25)] // Día posterior: ya cumplió
    public void Calcular_Nacido29FebreroEnAnioNoBisiesto_Regla28Febrero_CumpleEl28Febrero(
        string hoyTexto, int edadEsperada)
    {
        // Arrange
        var fechaNacimiento = new DateOnly(2000, 2, 29);
        var hoy = Fecha(hoyTexto);

        // Act
        var edad = Edad.Calcular(fechaNacimiento, hoy, Regla29Febrero.VeintiochoDeFebrero);

        // Assert
        Assert.Equal(edadEsperada, edad);
    }

    [Theory]
    [InlineData("2025-02-28", 24)] // Día anterior: aún no cumple
    [InlineData("2025-03-01", 25)] // Cumpleaños hoy según regla 1 de marzo
    [InlineData("2025-03-02", 25)] // Día posterior: ya cumplió
    public void Calcular_Nacido29FebreroEnAnioNoBisiesto_Regla1Marzo_CumpleEl1Marzo(
        string hoyTexto, int edadEsperada)
    {
        // Arrange
        var fechaNacimiento = new DateOnly(2000, 2, 29);
        var hoy = Fecha(hoyTexto);

        // Act
        var edad = Edad.Calcular(fechaNacimiento, hoy, Regla29Febrero.PrimeroDeMarzo);

        // Assert
        Assert.Equal(edadEsperada, edad);
    }

    [Theory]
    [InlineData(Regla29Febrero.VeintiochoDeFebrero, "2024-02-28", 23)] // Día anterior: aún no cumple
    [InlineData(Regla29Febrero.VeintiochoDeFebrero, "2024-02-29", 24)] // Cumpleaños hoy en año bisiesto
    [InlineData(Regla29Febrero.VeintiochoDeFebrero, "2024-03-01", 24)] // Día posterior: ya cumplió
    [InlineData(Regla29Febrero.PrimeroDeMarzo, "2024-02-28", 23)]
    [InlineData(Regla29Febrero.PrimeroDeMarzo, "2024-02-29", 24)]
    [InlineData(Regla29Febrero.PrimeroDeMarzo, "2024-03-01", 24)]
    public void Calcular_Nacido29FebreroEnAnioBisiesto_CumpleEl29FebreroIndependientementeDeRegla(
        Regla29Febrero regla, string hoyTexto, int edadEsperada)
    {
        // Arrange
        var fechaNacimiento = new DateOnly(2000, 2, 29);
        var hoy = Fecha(hoyTexto);

        // Act
        var edad = Edad.Calcular(fechaNacimiento, hoy, regla);

        // Assert
        Assert.Equal(edadEsperada, edad);
    }

    [Fact]
    public void Calcular_FechaNacimientoIgualAHoy_DevuelveCero()
    {
        // Arrange
        var fecha = new DateOnly(2026, 5, 15);

        // Act
        var edad = Edad.Calcular(fecha, fecha, Regla29Febrero.VeintiochoDeFebrero);

        // Assert
        Assert.Equal(0, edad);
    }

    [Theory]
    [InlineData("2026-05-15", "2026-05-14", Regla29Febrero.VeintiochoDeFebrero)] // Nació mañana
    [InlineData("2027-05-15", "2026-05-15", Regla29Febrero.VeintiochoDeFebrero)] // Nació el próximo año
    [InlineData("2030-01-01", "2026-05-15", Regla29Febrero.PrimeroDeMarzo)]       // Nació en varios años
    public void Calcular_FechaNacimientoPosteriorAHoy_DevuelveEdadNegativa(
        string fechaNacimientoTexto, string hoyTexto, Regla29Febrero regla)
    {
        // Arrange
        var fechaNacimiento = Fecha(fechaNacimientoTexto);
        var hoy = Fecha(hoyTexto);

        // Act
        var edad = Edad.Calcular(fechaNacimiento, hoy, regla);

        // Assert
        Assert.True(edad < 0, $"Se esperaba edad negativa, pero fue: {edad}");
    }

    private static DateOnly Fecha(string fecha) =>
        DateOnly.ParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture);
}

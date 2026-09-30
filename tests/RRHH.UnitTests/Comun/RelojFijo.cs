namespace RRHH.UnitTests.Comun;

public class RelojFijo : TimeProvider
{
    // Un año no bisiesto para que el 28 de febrero se pueda usar como "hoy" al probar cumpleaños del 29 de febrero
    private readonly DateTimeOffset _ahora = new(2027, 2, 28, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _ahora;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

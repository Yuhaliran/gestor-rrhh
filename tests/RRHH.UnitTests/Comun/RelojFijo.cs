namespace RRHH.UnitTests.Comun;

public class RelojFijo : TimeProvider
{
    // A non-leap year so 28 Feb can be used as "today" for testing 29 Feb birthdates
    private readonly DateTimeOffset _ahora = new(2027, 2, 28, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _ahora;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

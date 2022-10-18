using EdgeCases.Time;

namespace EdgeCases;

internal class Greeter
{
    private readonly IDateTimeProvider _dateTimeProvider;

    internal Greeter(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    internal string GenerateGreetText()
    {
        var dateTimeNow = _dateTimeProvider.Now;
        return dateTimeNow.Hour switch
        {
            >= 5 and < 12 => "Good morning",
            >= 12 and < 18 => "Good afternoon",
            _ => "Good evening"
        };
    }
}

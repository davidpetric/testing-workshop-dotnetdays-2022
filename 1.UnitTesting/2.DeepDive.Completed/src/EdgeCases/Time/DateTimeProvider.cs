namespace EdgeCases.Time;

internal interface IDateTimeProvider
{
    DateTime Now { get; }
    
    DateTime UtcNow { get; }
}

internal class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
    
    public DateTime UtcNow => DateTime.UtcNow;
}

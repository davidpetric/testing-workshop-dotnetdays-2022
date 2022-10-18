namespace ForeignExchange.Api.Logging;

public interface ILoggerAdapter<T>
{
    void LogInformation(string messageTemplate, params object?[] args);
}

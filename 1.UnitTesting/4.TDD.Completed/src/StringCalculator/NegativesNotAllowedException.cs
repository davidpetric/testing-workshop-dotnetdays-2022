namespace StringCalculator;

public class NegativesNotAllowedException : Exception
{
    public NegativesNotAllowedException(IEnumerable<int> negative) 
        : base($"Negative numbers such as {string.Join(", ", negative)} are not allowed.")
    {
        
    }
}

namespace StringCalculator;

public class Calculator
{
    public int Add(string numbers)
    {
        var separators = new List<char>{',', '\n'};
        
        if (numbers == string.Empty)
        {
            return 0;
        }

        if (numbers.StartsWith("//"))
        {
            var splitOnFirstNewLine = numbers.Split(new[] {'\n'}, 2);
            var customDelimiter = splitOnFirstNewLine[0].Replace("//", string.Empty).Single();
            separators.Add(customDelimiter);
            numbers = splitOnFirstNewLine[1];
        }
        
        var splitNumbers = numbers
            .Split(separators.ToArray())
            .Select(int.Parse)
            .Where(x => x <= 1000)
            .ToList();

        var negativeNumbers = splitNumbers.Where(x => x < 0).ToList();

        if (negativeNumbers.Any())
        {
            throw new NegativesNotAllowedException(negativeNumbers);
        }

        return splitNumbers.Sum();
    }
}

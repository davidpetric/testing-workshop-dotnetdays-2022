using System.Collections;

namespace AmazingCalculator.Tests.Unit;

public class AddData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var random = new Random(1000);
        for (int i = 0; i < 100; i++)
        {
            var a = random.Next(-1000, 1000);
            var b = random.Next(-1000, 1000);
            var result = a + b;
            yield return new object[] { a, b, result };
        }
        
        // yield return new object[] { 1, 2, 3 };
        // yield return new object[] { -5, 5, 0 };
        // yield return new object[] { -5, -5, -10 };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

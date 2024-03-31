namespace InDuckTor.Account.Domain;

public static class ControlNumbersGenerator
{
    /// <summary>
    /// Создаёт контрольное число
    /// </summary>
    /// <param name="numbers">Входящая последовательность чисел</param>
    /// <param name="weights">Веса</param>
    /// <param name="index">Позиция контрольного числа в последовательности</param>
    public static byte Create(ReadOnlySpan<byte> numbers, ReadOnlySpan<byte> weights, int index)
    {
        if (index >= numbers.Length)
            throw new ArgumentException("Позиция контрольного числа вне последовательности");
        
        var lastNumber = CalculateWeightedSum(numbers, weights) % 10;
        return (byte)((lastNumber * weights[index]) % 10);
    }

    /// <summary>
    /// Создаёт контрольное число
    /// </summary>
    /// <param name="numbers">Входящая последовательность чисел</param>
    /// <param name="weights">Веса</param>
    public static bool Verify(ReadOnlySpan<byte> numbers, ReadOnlySpan<byte> weights)
    {
        var weightedSum = CalculateWeightedSum(numbers, weights);
        return (weightedSum % 10) == 0;
    }

    private static long CalculateWeightedSum(ReadOnlySpan<byte> numbers, ReadOnlySpan<byte> weights)
    {
        if (numbers.Length > weights.Length)
            throw new ArgumentException("Размер входящей последовательности больше последовательности весов");

        long weightedSum = 0;
        for (int i = 0; i < numbers.Length; ++i)
        {
            weightedSum += numbers[i] * weights[i];
        }

        return weightedSum;
    }
}
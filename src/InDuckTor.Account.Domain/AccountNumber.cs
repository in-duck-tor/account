using System.Text;

namespace InDuckTor.Account.Domain;

public record struct AccountNumber(string Value) : IParsable<AccountNumber>
{
    /// <param name="bankCode">БИК к которому относится этот счёт</param>
    /// <param name="balanceAccountCode">Код балансового счёта 2 порядка</param>
    /// <param name="currencyNumericCode"></param>
    /// <param name="accountId">Внутренний номер счёта</param>
    public static AccountNumber CreatePaymentAccountNumber(BankCode bankCode, int balanceAccountCode, int currencyNumericCode, long accountId)
    {
        const int controlNumberIndex = 3 + 5 + 3;
        Span<byte> numbers = stackalloc byte[23];
        numbers[controlNumberIndex] = 0;
        TakeDigits(bankCode.Value, 3, numbers);

        var accountNumberData = numbers[3..];
        TakeDigits(balanceAccountCode, 5, accountNumberData);
        TakeDigits(currencyNumericCode, 3, accountNumberData[5..]);
        TakeDigits(accountId, 11, accountNumberData[(5 + 3 + 1)..]);

        numbers[controlNumberIndex] = ControlNumbersGenerator.Create(
            numbers,
            ControlNumberWeights,
            controlNumberIndex);

        for (int i = 0; i < accountNumberData.Length; ++i)
        {
            accountNumberData[i] += (byte)'0';
        }

        return Encoding.UTF8.GetString(accountNumberData);
    }

    private static void TakeDigits(long value, int digitCount, Span<byte> outDigits)
    {
        for (int i = digitCount - 1; i >= 0; --i)
        {
            outDigits[i] = (byte)(value % 10);
            value /= 10;
        }
    }

    private static void TakeDigits(string value, int digitCount, Span<byte> outDigits)
    {
        for (int i = digitCount - 1; i >= 0; --i)
        {
            outDigits[i] = (byte)value[i];
        }
    }

    public static bool VerifyControlNumber(AccountNumber accountNumber, BankCode bankCode)
    {
        Span<byte> numbers = stackalloc byte[23];
        TakeDigits(bankCode.Value, 3, numbers);
        TakeDigits(accountNumber.Value, 20, numbers[3..]);
        return ControlNumbersGenerator.Verify(numbers, ControlNumberWeights);
    }

    private static readonly byte[] ControlNumberWeights = [ 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1 ];

    public static implicit operator string(AccountNumber number) => number.Value;
    public static implicit operator AccountNumber(string stringNumber) => new(stringNumber);

    public static bool TryParse(string? value, out AccountNumber number)
        => (value?.Length != 20
               ? (number = default)
               : (number = new AccountNumber(value)))
           != default;

    public static AccountNumber Parse(string value, IFormatProvider? provider)
        => TryParse(value, out var result) ? result : throw new FormatException("Номер счёта должен состоять из 20 цифр");

    public static bool TryParse(string? value, IFormatProvider? formatProvider, out AccountNumber number) => TryParse(value, out number);
}
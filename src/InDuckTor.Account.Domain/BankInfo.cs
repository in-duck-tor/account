namespace InDuckTor.Account.Domain;

public class BankInfo
{
    /// <summary>
    /// БИК
    /// </summary>
    public required BankCode BankCode { get; init; }

    public string? Name { get; set; }
    public static readonly BankCode InDuckTorBankCode = default;
}

/// <summary>
/// БИК. 9-ти значный уникальный код банка 
/// </summary>
public record struct BankCode(int Value) : IParsable<BankCode>
{
    public static implicit operator string(BankCode code) => code.ToString();
    public static implicit operator int(BankCode code) => code.Value;
    public static implicit operator BankCode(string codeValue) => Parse(codeValue);

    public static BankCode Parse(string codeValue, IFormatProvider? provider = null)
        => TryParse(codeValue, out var result)
            ? result
            : throw new FormatException($"Неверный формат БИК, значение - {codeValue}");

    public static bool TryParse(string? codeValue, out BankCode result)
    {
        if (codeValue?.Length == 9 && int.TryParse(codeValue, out var value))
        {
            result = new BankCode(value);
            return true;
        }

        result = default;
        return false;
    }

    public static bool TryParse(string? codeValue, IFormatProvider? provider, out BankCode result) => TryParse(codeValue, out result);

    public override string ToString() => Value.ToString("D9");
    
    public bool IsExternal => Value != BankInfo.InDuckTorBankCode.Value;
}
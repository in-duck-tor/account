namespace InDuckTor.Account.Domain;

public class BankInfo
{
    /// <summary>
    /// БИК
    /// </summary>
    public required string BankCode { get; init; }

    public string? Name { get; set; }

    // todo create BIC value type
    public const string InDuckTorBankCode = "00000000";
}
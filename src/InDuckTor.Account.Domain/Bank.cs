namespace InDuckTor.Account.Domain;

public class Bank
{
    /// <summary>
    /// БИК
    /// </summary>
    public required int BankCode { get; init; }

    public string? Name { get; set; }

    // todo create BIC value type
    public const string InDuckTorBankCode = "00000000";
}
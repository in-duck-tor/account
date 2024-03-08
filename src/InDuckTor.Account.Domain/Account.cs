using System.Runtime.Serialization;

namespace InDuckTor.Account.Domain;

public class Account
{
    public required AccountNumber Number { get; init; }
    public required AccountType Type { get; init; }
    public required Currency Currency { get; init; }

    public string BankCode { get; init; } = Bank.InDuckTorBankCode;

    /// <summary>
    /// <b>ДЕНЬГИ</b>
    /// </summary>
    public decimal Amount { get; set; } = 0;

    public AccountState State { get; set; } = AccountState.Active;

    public required int CreatedBy { get; init; }

    /// <summary>
    /// Комментарий\записка назначении счёта при создании 
    /// </summary>
    public string? CustomComment { get; set; }

    /// <summary>
    /// "Пользователи" счёта
    /// </summary>
    public List<GrantedAccountUser> GrantedUsers { get; set; } = new();
}

public record struct AccountNumber(string Value)
{
    public AccountNumber CreatePaymentAccountNumber(int balanceAccountCode, int currencyNumericCode, long accountId)
    {
        throw new NotImplementedException();
    }

    public static implicit operator string(AccountNumber number) => number.Value;
}

public enum AccountType
{
    /// <summary>
    /// Расчётный счёт
    /// </summary>
    Payment,

    /// <summary>
    /// Ссудный счёт
    /// </summary>
    Loan,
}

public enum AccountState
{
    [EnumMember(Value = "active")] Active,
    [EnumMember(Value = "closed")] Closed,
    [EnumMember(Value = "frozen")] Frozen
}

/// <summary>
/// Действие над счётом
/// </summary>
public enum AccountAction
{
    /// <summary>
    /// Внесение средств	
    /// </summary>
    Deposit,

    /// <summary>
    /// Вывод средств
    /// </summary>
    Withdraw,

    /// <summary>
    /// Заморозить счёт
    /// </summary>
    Froze,
}

/// <summary>
/// Особые права пользователя на действие со счётом
/// </summary>
/// <remarks>Вносить средства могут все по умолчанию</remarks>
public record GrantedAccountUser(int Id, AccountAction[] Actions);
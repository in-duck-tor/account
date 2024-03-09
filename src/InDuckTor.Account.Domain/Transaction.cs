using System.Runtime.Serialization;

namespace InDuckTor.Account.Domain;

// todo hierarchy for TransactionType
public class Transaction
{
    public int Id { get; init; }

    public required TransactionType Type { get; init; }

    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    public DateTime StartedAt { get; init; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }

    public TransactionTarget? DepositOn { get; init; }

    public TransactionTarget? WithdrawFrom { get; init; }

    /// <summary>
    /// Резервации средств га счётах 
    /// </summary>
    public FundsReservation[] Reservations { get; init; } = Array.Empty<FundsReservation>();
}

public enum TransactionType
{
    /// <summary>
    /// Снятие
    /// </summary>
    [EnumMember(Value = "withdraw")] Withdraw,

    /// <summary>
    /// Внесение
    /// </summary>
    [EnumMember(Value = "deposit")] Deposit,

    /// <summary>
    /// Перевод
    /// </summary>
    [EnumMember(Value = "transfer")] Transfer,

    /// <summary>
    /// Перевод на внешний счёт
    /// </summary>
    [EnumMember(Value = "transfer_to_external")]
    TransferToExternal,

    /// <summary>
    /// Перевод со внешнего счёт
    /// </summary>
    [EnumMember(Value = "transfer_from_external")]
    TransferFromExternal
}

public enum TransactionStatus
{
    /// <summary>
    /// Трансакция обрабатывается
    /// </summary>
    [EnumMember(Value = "pending")]
    Pending,
    /// <summary>
    /// Трансакция исполнена
    /// </summary>
    [EnumMember(Value = "committed")]
    Committed,
    /// <summary>
    /// Трансакция отменена
    /// </summary>
    [EnumMember(Value = "canceled")]
    Canceled,
}

/// <param name="BankCode">БИК</param>
public record TransactionTarget(decimal Amount, AccountNumber Account, string BankCode);
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using FluentResults;

namespace InDuckTor.Account.Domain;

// todo hierarchy for TransactionType
public class Transaction
{
    public long Id { get; init; }

    public required TransactionType Type { get; init; }

    public TransactionStatus Status { get; set; } = TransactionStatus.Creating;

    public required int InitiatedBy { get; init; }

    public DateTime StartedAt { get; init; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public required DateTime AutoCloseAt { get; set; }

    public TransactionTarget? DepositOn { get; init; }

    public TransactionTarget? WithdrawFrom { get; init; }

    /// <summary>
    /// Резервации средств га счётах 
    /// </summary>
    public FundsReservation[] Reservations { get; private set; } = Array.Empty<FundsReservation>();

    public Result<Transaction> AddReservations(params FundsReservation[] reservations)
    {
        if (Status != TransactionStatus.Creating)
            return new Errors.Conflict("Невозможно применить резервацию средств на счёту по уже созданной трансакции");

        Reservations = Reservations.Concat(reservations).ToArray();
        return this;
    }

    // для нужд EF 
    protected Transaction()
    {
    }

    public static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);

    /// <exception cref="InvalidOperationException">Когда оба аргумента <c>null</c></exception>
    [SetsRequiredMembers]
    public Transaction(TransactionTarget? depositOn, TransactionTarget? withdrawFrom, TimeSpan ttl, int initiatedBy)
    {
        DepositOn = depositOn;
        WithdrawFrom = withdrawFrom;
        InitiatedBy = initiatedBy;
        AutoCloseAt = DateTime.UtcNow.Add(ttl);
        Type = GetTransactionType(depositOn, withdrawFrom);
    }

    private static TransactionType GetTransactionType(TransactionTarget? depositOn, TransactionTarget? withdrawFrom)
    {
        // @formatter:off
        if (withdrawFrom is { BankCode: BankInfo.InDuckTorBankCode } && depositOn is { BankCode: BankInfo.InDuckTorBankCode }) return TransactionType.Transfer;
        if (withdrawFrom is { BankCode: BankInfo.InDuckTorBankCode } && depositOn is not null )                                return TransactionType.TransferToExternal;
        if (withdrawFrom is { BankCode: BankInfo.InDuckTorBankCode } && depositOn is null)                                     return TransactionType.Withdraw;
        if (withdrawFrom is not null                                 && depositOn is { BankCode: BankInfo.InDuckTorBankCode }) return TransactionType.TransferFromExternal;
        if (withdrawFrom is null                                     && depositOn is { BankCode: BankInfo.InDuckTorBankCode }) return TransactionType.Deposit;
        //@formatter:on
        throw new InvalidOperationException("Невозможно совершить операцию не указав известные счёта отправителя и получателя");
    }

    public Result Commit()
    {
        if (Status != TransactionStatus.Pending)
            return new Errors.Conflict("Невозможно завершить неактивную трансакции");

        Status = TransactionStatus.Committed;
        FinishedAt = DateTime.UtcNow;
        Reservations = [ ];

        return Result.Ok();
    }

    public Result Cancel()
    {
        if (Status != TransactionStatus.Pending)
            return new Errors.Conflict("Невозможно завершить неактивную трансакции");

        Status = TransactionStatus.Canceled;
        FinishedAt = DateTime.UtcNow;
        Reservations = [ ];

        return Result.Ok();
    }
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
    /// Трансакция в процессе создания 
    /// </summary>
    [EnumMember(Value = "creating")] Creating,

    /// <summary>
    /// Трансакция обрабатывается
    /// </summary>
    [EnumMember(Value = "pending")] Pending,

    /// <summary>
    /// Трансакция исполнена
    /// </summary>
    [EnumMember(Value = "committed")] Committed,

    /// <summary>
    /// Трансакция отменена
    /// </summary>
    [EnumMember(Value = "canceled")] Canceled,
}

/// <param name="BankCode">БИК</param>
public record TransactionTarget(decimal Amount, AccountNumber AccountNumber, string CurrencyCode, string BankCode)
{
    public BankInfo BankInfo { get; init; } = null!;

    public bool InExternal => BankCode != BankInfo.InDuckTorBankCode;
}
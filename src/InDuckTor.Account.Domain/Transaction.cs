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
    public IReadOnlyCollection<FundsReservation> Reservations
    {
        get => _reservations;
        init => _reservations = value.ToList();
    }

    private List<FundsReservation> _reservations;

    public Result<Transaction> AddReservations(params FundsReservation[] reservations)
    {
        if (Status != TransactionStatus.Creating)
            return new Errors.Conflict("Невозможно применить резервацию средств на счёту по уже созданной трансакции");

        _reservations.AddRange(reservations);
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
        _reservations = new List<FundsReservation>();
    }

    private static TransactionType GetTransactionType(TransactionTarget? depositOn, TransactionTarget? withdrawFrom)
    {
        // @formatter:off
        if (withdrawFrom?.BankCode == BankInfo.InDuckTorBankCode && depositOn?.BankCode == BankInfo.InDuckTorBankCode) return TransactionType.Transfer;
        if (withdrawFrom?.BankCode == BankInfo.InDuckTorBankCode && depositOn is not null )                            return TransactionType.TransferToExternal;
        if (withdrawFrom?.BankCode == BankInfo.InDuckTorBankCode && depositOn is null)                                 return TransactionType.Withdraw;
        if (withdrawFrom is not null                             && depositOn?.BankCode == BankInfo.InDuckTorBankCode) return TransactionType.TransferFromExternal;
        if (withdrawFrom is null                                 && depositOn?.BankCode == BankInfo.InDuckTorBankCode) return TransactionType.Deposit;
        //@formatter:on
        throw new InvalidOperationException("Невозможно совершить операцию не указав известные счёта отправителя и получателя");
    }

    public Result<FundsReservation[]> Commit()
    {
        if (Status != TransactionStatus.Pending)
            return new Errors.Conflict("Невозможно завершить неактивную трансакции");

        Status = TransactionStatus.Committed;
        FinishedAt = DateTime.UtcNow;

        var committedReservations = _reservations.ToArray();
        _reservations.Clear();

        return committedReservations;
    }

    public Result<FundsReservation[]> Cancel()
    {
        if (Status != TransactionStatus.Pending)
            return new Errors.Conflict("Невозможно завершить неактивную трансакции");

        Status = TransactionStatus.Canceled;
        FinishedAt = DateTime.UtcNow;

        var cancelledReservations = _reservations.ToArray();
        _reservations.Clear();

        return cancelledReservations;
    }
}

public enum TransactionType
{
    /// <summary>
    /// Снятие
    /// </summary>
    [EnumMember(Value = "withdraw")] Withdraw = 1,

    /// <summary>
    /// Внесение
    /// </summary>
    [EnumMember(Value = "deposit")] Deposit = 2,

    /// <summary>
    /// Перевод
    /// </summary>
    [EnumMember(Value = "transfer")] Transfer = 3,

    /// <summary>
    /// Перевод на внешний счёт
    /// </summary>
    [EnumMember(Value = "transfer_to_external")]
    TransferToExternal = 4,

    /// <summary>
    /// Перевод со внешнего счёт
    /// </summary>
    [EnumMember(Value = "transfer_from_external")]
    TransferFromExternal = 5
}

public enum TransactionStatus
{
    /// <summary>
    /// Трансакция в процессе создания 
    /// </summary>
    [EnumMember(Value = "creating")] Creating = 1,

    /// <summary>
    /// Трансакция обрабатывается
    /// </summary>
    [EnumMember(Value = "pending")] Pending = 2,

    /// <summary>
    /// Трансакция исполнена
    /// </summary>
    [EnumMember(Value = "committed")] Committed = 3,

    /// <summary>
    /// Трансакция отменена
    /// </summary>
    [EnumMember(Value = "canceled")] Canceled = 4,
}

/// <param name="BankCode">БИК</param>
/// <param name="CurrencyCode">Код валюты, если известен</param>
public record TransactionTarget(decimal Amount, AccountNumber AccountNumber, string? CurrencyCode, BankCode BankCode)
{
    public BankInfo BankInfo { get; init; } = null!;

    public bool InExternal => BankCode != BankInfo.InDuckTorBankCode;
}
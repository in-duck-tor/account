using System.Runtime.Serialization;
using FluentResults;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Security.Context;
using UserAccountType = InDuckTor.Shared.Security.Context.AccountType;

namespace InDuckTor.Account.Domain;

public class Account
{
    public required AccountNumber Number { get; init; }
    public required AccountType Type { get; init; }
    public required string CurrencyCode { get; init; }
    public Currency Currency { get; init; }

    /// <summary>
    /// Пользователь-владелец для которого был создан счёт
    /// </summary>
    public required int OwnerId { get; init; }

    /// <summary>
    /// Пользователь-создатель может быть как клиент так и система\сервисный аккаунт 
    /// </summary>
    public required int CreatedBy { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public BankCode BankCode { get; init; } = BankInfo.InDuckTorBankCode;
    public BankInfo BankInfo { get; set; }

    /// <summary>
    /// <b>ДЕНЬГИ</b>
    /// </summary>
    public decimal Amount { get; set; } = 0;

    public AccountState State { get; set; } = AccountState.Active;

    /// <summary>
    /// Комментарий\записка назначении счёта при создании 
    /// </summary>
    public string? CustomComment { get; set; }

    /// <summary>
    /// "Пользователи" счёта
    /// </summary>
    public List<GrantedAccountUser> GrantedUsers { get; set; } = new();

    public bool IsActive => State == AccountState.Active;

    public bool CanUserRead(UserContext userContext)
        => HasUserAction(userContext.Id, AccountAction.ReadOperations)
           || userContext.Permissions.Contains(Permission.Account.Read)
           || userContext.AccountType == UserAccountType.System;

    public bool CanUserWithdraw(UserContext userContext)
        => IsActive && (HasUserAction(userContext.Id, AccountAction.Withdraw)
                        || userContext.AccountType == UserAccountType.System);

    public bool CanUserDeposit(UserContext userContext)
        => IsActive && (Type == AccountType.Payment
                        || userContext.AccountType == UserAccountType.System);

    public bool CanUserFreeze(UserContext userContext)
        => IsActive && (HasUserAction(userContext.Id, AccountAction.Freeze)
                        || userContext.Permissions.Contains(Permission.Account.Manage)
                        || userContext.AccountType == UserAccountType.System);

    public bool CanUserClose(UserContext userContext)
        => IsActive && (HasUserAction(userContext.Id, AccountAction.Close)
                        || userContext.Permissions.Contains(Permission.Account.Manage)
                        || userContext.AccountType == UserAccountType.System);

    private bool HasUserAction(int userId, AccountAction action)
        => GrantedUsers.Any(granted => granted.Id == userId && granted.Actions.Contains(action));

    public Result Freeze()
    {
        if (State == AccountState.Closed) return new Errors.Conflict("Счёт уже закрыт");
        State = AccountState.Frozen;
        return Result.Ok();
    }

    public Result Unfreeze()
    {
        if (State != AccountState.Frozen) return new Errors.Conflict("Счёт не заморожен");
        State = AccountState.Active;
        return Result.Ok();
    }

    public Result Close()
    {
        if (State == AccountState.Closed) return new Errors.Conflict("Счёт уже закрыт");
        State = AccountState.Closed;
        return Result.Ok();
    }
}

public enum AccountType
{
    /// <summary>
    /// Расчётный счёт
    /// </summary>
    [EnumMember(Value = "payment")] Payment = 1,

    /// <summary>
    /// Ссудный счёт
    /// </summary>
    [EnumMember(Value = "loan")] Loan = 2,

    /// <summary>
    /// Касса из которой ведется расчёт наличными; относится к счёту 20202 
    /// </summary>
    [EnumMember(Value = "cash_register")] CashRegister = 3,
}

public enum AccountState
{
    [EnumMember(Value = "active")] Active = 1,
    [EnumMember(Value = "closed")] Closed = 2,
    [EnumMember(Value = "frozen")] Frozen = 3
}

/// <summary>
/// Действие над счётом
/// </summary>
public enum AccountAction
{
    /// <summary>
    /// Внесение средств	
    /// </summary>
    [EnumMember(Value = "deposit")] Deposit = 1,

    /// <summary>
    /// Вывод средств
    /// </summary>
    [EnumMember(Value = "withdraw")] Withdraw = 2,

    /// <summary>
    /// Заморозить счёт
    /// </summary>
    [EnumMember(Value = "freeze")] Freeze = 3,

    /// <summary>
    /// Закрыть счёт
    /// </summary>
    [EnumMember(Value = "close")] Close = 4,

    /// <summary>
    /// Читать операции по счёту
    /// </summary>
    [EnumMember(Value = "read")] ReadOperations = 5
}

/// <summary>
/// Особые права пользователя на действие со счётом
/// </summary>
/// <remarks>Вносить средства могут все по умолчанию</remarks>
public record GrantedAccountUser(int Id, AccountAction[] Actions);
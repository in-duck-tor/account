using InDuckTor.Account.Domain;

namespace InDuckTor.Account.WebApi.Contracts;

/// <param name="Amount"></param>
/// <param name="AccountNumber"></param>
/// <param name="BankCode">БИК</param>
/// <param name="BankName">Имя банка</param>
public record TransactionTargetDto(decimal Amount, string AccountNumber, string BankCode, string? BankName);

public record TransactionDto(
    int Id,
    TransactionType Type,
    TransactionStatus Status,
    TransactionTargetDto? DepositOn,
    TransactionTargetDto? WithdrawFrom);
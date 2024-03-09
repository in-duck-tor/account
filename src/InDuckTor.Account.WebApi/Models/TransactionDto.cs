using InDuckTor.Account.Domain;

namespace InDuckTor.Account.WebApi.Models;

public record TransactionDto(
    int Id,
    TransactionType Type,
    TransactionStatus Status,
    DateTime StartedAt,
    DateTime? FinishedAt,
    TransactionDto.Target? DepositOn,
    TransactionDto.Target? WithdrawFrom)
{
    /// <param name="Amount"><b>ДЕНЬГИ</b></param>
    /// <param name="AccountNumber"></param>
    /// <param name="BankCode">БИК</param>
    /// <param name="BankName">Имя банка</param>
    public record Target(decimal Amount, string AccountNumber, string BankCode, string? BankName);
}
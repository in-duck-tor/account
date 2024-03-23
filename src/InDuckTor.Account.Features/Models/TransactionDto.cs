using System.Linq.Expressions;
using InDuckTor.Account.Domain;

namespace InDuckTor.Account.Features.Models;

public record TransactionDto(
    long Id,
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
    /// <param name="CurrencyCode">Трёхбуквенный алфавитный код Валюты (ISO 4217)</param>
    public record Target(decimal Amount, string? CurrencyCode, string AccountNumber, BankCode BankCode, string? BankName);

    public static Expression<Func<Transaction, TransactionDto>> Projection
        => x => new TransactionDto(
            x.Id,
            x.Type,
            x.Status,
            x.StartedAt,
            x.FinishedAt,
            x.DepositOn != null
                ? new Target(x.DepositOn.Amount, x.DepositOn.CurrencyCode, x.DepositOn.AccountNumber, x.DepositOn.BankCode, x.DepositOn.BankInfo.Name)
                : null,
            x.WithdrawFrom != null
                ? new Target(x.WithdrawFrom.Amount, x.WithdrawFrom.CurrencyCode, x.WithdrawFrom.AccountNumber, x.WithdrawFrom.BankCode, x.WithdrawFrom.BankInfo.Name)
                : null);
}
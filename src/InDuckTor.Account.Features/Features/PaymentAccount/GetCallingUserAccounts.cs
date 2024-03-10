using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;
using Microsoft.EntityFrameworkCore;
using AccountType = InDuckTor.Account.Domain.AccountType;

namespace InDuckTor.Account.Features.PaymentAccount;

/// <param name="Number">Номер счёта</param>
/// <param name="CurrencyCode">Трёхбуквенный алфавитный код Валюты (ISO 4217)</param>
/// <param name="Amount"><b>ДЕНЬГИ</b></param>
/// <param name="State">Статус счёта</param>
/// <param name="CustomComment">Комментарий к счёту оставленный при создании</param>
public record PaymentAccountDto(string Number, string CurrencyCode, decimal Amount, AccountState State, string? CustomComment);

public interface IGetCallingUserAccounts : IQuery<Unit, PaymentAccountDto[]>;

public class GetCallingUserAccounts(AccountsDbContext context, ISecurityContext securityContext) : IGetCallingUserAccounts
{
    public Task<PaymentAccountDto[]> Execute(Unit input, CancellationToken ct)
    {
        var currantUserId = securityContext.Currant.Id;
        return context.Accounts.Where(x => x.Type == AccountType.Payment && x.OwnerId == currantUserId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new PaymentAccountDto(
                x.Number,
                x.Currency.Code,
                x.Amount,
                x.State,
                x.CustomComment))
            .ToArrayAsync(ct);
    }
}
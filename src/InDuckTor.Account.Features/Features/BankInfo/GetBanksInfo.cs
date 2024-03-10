using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Strategies;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Account.Features.BankInfo;

/// <param name="BankCode">БИК</param>
/// <param name="Name"></param>
public record BankInfo(string BankCode, string? Name);

public interface IGetBanksInfo : IQuery<Unit, BankInfo[]>;

public class GetBanksInfo(AccountsDbContext context) : IGetBanksInfo
{
    public Task<BankInfo[]> Execute(Unit input, CancellationToken ct)
    {
        // todo cache 
        return context.Banks.Select(x => new BankInfo(x.BankCode, x.Name)).ToArrayAsync(ct);
    }
}
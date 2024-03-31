using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Strategies;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Account.Features.BankInfo;

/// <param name="Code">Трёхбуквенный алфавитный код ISO 4217</param>
/// <param name="NumericCode">Трёхзначный цифровой код ISO 4217</param>
/// <param name="ExchangeRate">Курс валюты к рублю</param>
public record CurrencyInfo(string Code, int NumericCode, decimal ExchangeRate);

public interface IGetCurrenciesInfo : IQuery<Unit, CurrencyInfo[]>;

public class GetCurrenciesInfo(AccountsDbContext context) : IGetCurrenciesInfo
{
    public Task<CurrencyInfo[]> Execute(Unit input, CancellationToken ct)
    {
        // todo cache 
        return context.Currencies.Select(x => new CurrencyInfo(x.Code, x.NumericCode, x.RateToRuble)).ToArrayAsync(ct);
    }
}
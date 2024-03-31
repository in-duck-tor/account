using InDuckTor.Account.Cbr.Integration;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Strategies;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Account.Features.BankInfo;

public interface IPullCbCurrencyRoutine : ICommand<Unit, Unit>;

public class PullCbCurrencyRoutine(ICbrClient cbrClient, AccountsDbContext context) : IPullCbCurrencyRoutine
{
    // todo : move to config
    private const string CbrTimeZoneId = "Russian Standard Time";

    public async Task<Unit> Execute(Unit input, CancellationToken ct)
    {
        var cbrNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(CbrTimeZoneId));
        var valCurs = await cbrClient.GetCurrencies(DateOnly.FromDateTime(cbrNow), ct);

        var currencies = valCurs.Valute
            .Select(valute => new Currency
            {
                Code = valute.CharCode,
                NumericCode = valute.NumCode,
                RateToRuble = decimal.Parse(valute.Value)
            })
            .ToList();

        await using var tx = await context.Database.BeginTransactionAsync(ct);

        // todo : may be upsert ?
        var dbCurrencies = await context.Currencies
            .ToDictionaryAsync(x => x.NumericCode, ct);

        foreach (var currency in currencies)
        {
            if (!dbCurrencies.TryGetValue(currency.NumericCode, out var dbCurrency)) continue;
            dbCurrency.RateToRuble = currency.NumericCode;
        }

        context.Currencies.AddRange(currencies.Where(c => !dbCurrencies.ContainsKey(c.NumericCode)));

        // todo : logging
        await context.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        return new Unit();
    }
}
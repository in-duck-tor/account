using System.Globalization;
using InDuckTor.Account.Cbr.Integration;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Strategies;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Account.Features.BankInfo;

public interface IPullCbrCurrencyRoutine : ICommand<Unit, Unit>;

public class PullCbrCurrencyRoutine(ICbrClient cbrClient, AccountsDbContext context) : IPullCbrCurrencyRoutine
{
    // todo : move to config
    private const string CbrTimeZoneId = "Russian Standard Time";
    private static readonly NumberFormatInfo NumberFormat = new() { NumberDecimalSeparator = "," };

    public async Task<Unit> Execute(Unit input, CancellationToken ct)
    {
        var cbrNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(CbrTimeZoneId));
        var valCurs = await cbrClient.GetCurrencies(DateOnly.FromDateTime(cbrNow), ct);
        
        var currencies = valCurs.Valute
            .Select(valute => new Currency
            {
                Code = valute.CharCode,
                NumericCode = valute.NumCode,
                RateToRuble = decimal.Parse(valute.Value, NumberFormat)
            })
            .ToList();

        await using var tx = await context.Database.BeginTransactionAsync(ct);

        // todo : may be upsert ?
        var dbCurrencies = await context.Currencies
            .ToDictionaryAsync(x => x.NumericCode, ct);

        foreach (var currency in currencies)
        {
            if (!dbCurrencies.TryGetValue(currency.NumericCode, out var dbCurrency)) continue;
            dbCurrency.RateToRuble = currency.RateToRuble;
        }

        context.Currencies.AddRange(currencies.Where(c => !dbCurrencies.ContainsKey(c.NumericCode)));

        // todo : logging
        await context.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        return new Unit();
    }
}
using System.Diagnostics.CodeAnalysis;

namespace InDuckTor.Account.Domain;

/// <summary>
/// Валюта
/// </summary>
public class Currency
{
    /// <summary>
    /// Трёхбуквенный алфавитный код ISO 4217
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Трёхзначный цифровой код ISO 4217 
    /// </summary>
    public required int NumericCode { get; init; }

    /// <summary>
    /// Отношение единицы валюты к рублю 
    /// </summary>
    public required decimal RateToRuble { get; set; }
}

// todo : config it as ef complex type 
public record struct Money(decimal Amount, string CurrencyCode)
{
    public Currency Currency { get; private init; }

    public Money(decimal amount, Currency currency) : this(amount, currency.Code) => Currency = currency;

    public Money TransferTo(Currency currency) => new(Amount * Currency.RateToRuble / currency.RateToRuble, currency);

    [DoesNotReturn]
    private static Money ThrowCurrencyMismatch() => throw new InvalidOperationException("Невозножно произвести операцию над разными валютами");

    public static Money operator +(Money left, Money right) => left.CurrencyCode == right.CurrencyCode ? new Money(left.Amount + right.Amount, left.CurrencyCode) { Currency = left.Currency ?? right.Currency } : ThrowCurrencyMismatch();
    public static Money operator -(Money left, Money right) => left.CurrencyCode == right.CurrencyCode ? new Money(left.Amount - right.Amount, left.CurrencyCode) { Currency = left.Currency ?? right.Currency } : ThrowCurrencyMismatch();
}
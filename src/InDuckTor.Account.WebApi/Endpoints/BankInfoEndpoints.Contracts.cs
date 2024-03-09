namespace InDuckTor.Account.WebApi.Endpoints;

/// <param name="BankCode">БИК</param>
/// <param name="Name"></param>
public record BankInfo(string BankCode, string? Name);

/// <param name="Code">Трёхбуквенный алфавитный код ISO 4217</param>
/// <param name="NumericCode">Трёхзначный цифровой код ISO 4217</param>
/// <param name="ExchangeRate">Курс валюты к рублю</param>
public record CurrencyInfo(string Code, int NumericCode, decimal ExchangeRate);
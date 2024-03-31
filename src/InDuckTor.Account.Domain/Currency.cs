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
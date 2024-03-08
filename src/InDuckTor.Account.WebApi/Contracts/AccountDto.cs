using InDuckTor.Account.Domain;

namespace InDuckTor.Account.WebApi.Contracts;

/// <param name="Number">Номер счёта</param>
/// <param name="CurrencyCode">Трёхбуквенный алфавитный код Валюты</param>
/// <param name="Amount"><b>ДЕНЬГИ</b></param>
/// <param name="State">Статус счёта</param>
/// <param name="CustomComment">Комментарий к счёту оставленный при создании</param>
public record AccountDto(string Number, string CurrencyCode, decimal Amount, AccountState State, string? CustomComment);
using InDuckTor.Account.Domain;

namespace InDuckTor.Account.WebApi.Endpoints;

public record AccountsSearchParams(
    int? OwnerId,
    AccountState? AccountState,
    AccountType? AccountType,
    int? Take,
    int? Skip);

/// <param name="Number">Номер счёта</param>
/// <param name="CurrencyCode">Трёхбуквенный алфавитный код Валюты (ISO 4217)</param>
/// <param name="BankCode">БИК</param>
/// <param name="Amount"><b>ДЕНЬГИ</b></param>
/// <param name="State">Статус счёта</param>
/// <param name="Type">Тип счёта</param>
/// <param name="CustomComment">Комментарий к счёту оставленный при создании</param>
/// <param name="GrantedUsers">Особые права пользователей на действие со счётом</param>
public record AccountDto(
    string Number,
    string CurrencyCode,
    string BankCode,
    decimal Amount,
    AccountState State,
    AccountType Type,
    string? CustomComment,
    AccountDto.GrantedUser[] GrantedUsers)
{
    public record GrantedUser(int Id, AccountAction[] Actions);
}
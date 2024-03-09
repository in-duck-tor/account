using InDuckTor.Account.Domain;

namespace InDuckTor.Account.WebApi.Endpoints;

/// <param name="Number">Номер счёта</param>
/// <param name="CurrencyCode">Трёхбуквенный алфавитный код Валюты (ISO 4217)</param>
/// <param name="Amount"><b>ДЕНЬГИ</b></param>
/// <param name="State">Статус счёта</param>
/// <param name="CustomComment">Комментарий к счёту оставленный при создании</param>
public record PaymentAccountDto(string Number, string CurrencyCode, decimal Amount, AccountState State, string? CustomComment);

/// <param name="CurrencyCode">Трёхбуквенный алфавитный код Валюты (ISO 4217)</param>
/// <param name="CustomComment">Комментарий к счёту который может оставить создатель</param>
public record OpenPaymentAccountRequest(string CurrencyCode, string? CustomComment);

public record MakeTransactionRequest(TransactionType Type, MakeTransactionRequest.Target? DepositOn, MakeTransactionRequest.Target? WithdrawFrom)
{
    /// <param name="Amount"><b>ДЕНЬГИ</b></param>
    /// <param name="BankCode">БИК</param>
    public record Target(decimal Amount, string AccountNumber, string BankCode);
}
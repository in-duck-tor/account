using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;
using AccountType = InDuckTor.Account.Domain.AccountType;

namespace InDuckTor.Account.Features.PaymentAccount;

/// <param name="CurrencyCode">Трёхбуквенный алфавитный код Валюты (ISO 4217)</param>
/// <param name="CustomComment">Комментарий к счёту который может оставить создатель</param>
public record OpenPaymentAccountRequest(string CurrencyCode, string? CustomComment);

public interface IOpenNewAccount : ICommand<OpenPaymentAccountRequest, Result<CreateAccountResult>>;

public class OpenNewAccount(
    AccountsDbContext context,
    ISecurityContext securityContext,
    ICreateNewAccountNumber createNewAccountNumber) : IOpenNewAccount
{
    public async Task<Result<CreateAccountResult>> Execute(OpenPaymentAccountRequest input, CancellationToken ct)
    {
        var currency = await context.Currencies.FindAsync([ input.CurrencyCode ], ct);
        if (currency is null) return new DomainErrors.Currency.NotFound(input.CurrencyCode);

        var accountNumber = await createNewAccountNumber.Execute(new NewAccountNumberArgs(AccountType.Payment, Domain.BankInfo.InDuckTorBankCode, currency), ct);
        var callingUserId = securityContext.Currant.Id;

        var account = new Domain.Account
        {
            Number = accountNumber,
            Currency = currency,
            CurrencyCode = currency.Code,
            Type = AccountType.Payment,
            CreatedBy = callingUserId,
            OwnerId = callingUserId,
            BankCode = Domain.BankInfo.InDuckTorBankCode,
            GrantedUsers = [ new GrantedAccountUser(callingUserId, AllAccountActions) ],
            CustomComment = input.CustomComment
        };

        context.Add(account);
        await context.SaveChangesAsync(ct);

        return new CreateAccountResult(account.Number);
    }

    private static readonly AccountAction[] AllAccountActions = [ AccountAction.Withdraw, AccountAction.Freeze, AccountAction.ReadOperations, AccountAction.Close ];
}
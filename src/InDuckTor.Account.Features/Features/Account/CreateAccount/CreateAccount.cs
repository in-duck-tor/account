using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;

namespace InDuckTor.Account.Features.Account.CreateAccount;

// todo check dependency registration if use internal implementation
[Intercept(typeof(RequireAccountManagementPermission<CreateAccountRequest, CreateAccountResult>))]
public class CreateAccount : ICreateAccount
{
    private readonly AccountsDbContext _context;
    private readonly ICreateNewAccountNumber _createNewAccountNumber;
    private readonly ISecurityContext _securityContext;

    public CreateAccount(AccountsDbContext context, ICreateNewAccountNumber createNewAccountNumber, ISecurityContext securityContext)
    {
        _context = context;
        _createNewAccountNumber = createNewAccountNumber;
        _securityContext = securityContext;
    }

    public async Task<Result<CreateAccountResult>> Execute(CreateAccountRequest input, CancellationToken ct)
    {
        var currency = await _context.Currencies.FindAsync([ input.CurrencyCode ], ct);
        if (currency is null) return new Errors.Currency.NotFound(input.CurrencyCode);

        var accountNumber = await _createNewAccountNumber.Execute(new NewAccountNumberArgs(input.AccountType, currency, input.PlannedExpiration), ct);

        var account = new Domain.Account
        {
            Number = accountNumber,
            Currency = currency,
            CurrencyCode = currency.Code,
            Type = input.AccountType,
            CreatedBy = _securityContext.Currant.Id,
            OwnerId = input.ForUserId,
            CustomComment = input.CustomComment,
            BankCode = Domain.BankInfo.InDuckTorBankCode,
            GrantedUsers = [ new GrantedAccountUser(input.ForUserId, [ AccountAction.Freeze, AccountAction.Withdraw, AccountAction.Read ]) ]
        };

        _context.Add(account);
        await _context.SaveChangesAsync(ct);

        return new CreateAccountResult(account.Number);
    }
}
using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies.Interceptors;

namespace InDuckTor.Account.Features.Account.CreateAccount;

[AllowSystem]
[RequirePermission(Permission.Account.Manage)]
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
        if (currency is null) return new DomainErrors.Currency.NotFound(input.CurrencyCode);

        var newAccountNumberArgs = new NewAccountNumberArgs(input.AccountType, Domain.BankInfo.InDuckTorBankCode, currency, input.PlannedExpiration);
        var accountNumber = await _createNewAccountNumber.Execute(newAccountNumberArgs, ct);

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
            GrantedUsers = [ new GrantedAccountUser(input.ForUserId, AllAccountActions) ]
        };

        _context.Add(account);
        await _context.SaveChangesAsync(ct);

        return new CreateAccountResult(account.Number);
    }

    private static readonly AccountAction[] AllAccountActions = [ AccountAction.Withdraw, AccountAction.Freeze, AccountAction.ReadOperations, AccountAction.Close ];
}
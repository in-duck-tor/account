using InDuckTor.Account.Domain;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Strategies;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Account.Features.Common;

public record struct NewAccountNumberArgs(AccountType AccountType, BankCode BankCode, Currency Currency, DateTime? PlannedExpiration = null);

public interface ICreateNewAccountNumber : IStrategy<NewAccountNumberArgs, AccountNumber>;

public class CreateNewAccountNumber(AccountsDbContext dbContext) : ICreateNewAccountNumber
{
    public async Task<AccountNumber> Execute(NewAccountNumberArgs input, CancellationToken ct)
    {
        var accountPersonalCode = await GetNextAccountPersonalCode(ct);
        var balanceAccountCode = input.AccountType switch
        {
            AccountType.Payment => КодыБалансовыхСчётов.ПрочиеСчёта.ФизическиеЛица,
            AccountType.Loan => GetLoanAccountCode(input.PlannedExpiration),
            _ => throw new ArgumentOutOfRangeException()
        };

        return AccountNumber.CreatePaymentAccountNumber(input.BankCode, balanceAccountCode, input.Currency.NumericCode, accountPersonalCode);
    }

    private int GetLoanAccountCode(DateTime? plannedExpiration)
    {
        var duration = plannedExpiration - DateTime.UtcNow;
        return duration?.Days switch
        {
            < 30 => КодыБалансовыхСчётов.КредитыФизическимЛицам.ДоМесяца,
            < 90 => КодыБалансовыхСчётов.КредитыФизическимЛицам.До3Месяцев,
            < 180 => КодыБалансовыхСчётов.КредитыФизическимЛицам.ДоПолуГода,
            < 365 => КодыБалансовыхСчётов.КредитыФизическимЛицам.ДоГода,
            < 365 * 3 => КодыБалансовыхСчётов.КредитыФизическимЛицам.До3Лет,
            > 365 * 3 => КодыБалансовыхСчётов.КредитыФизическимЛицам.Более3Лет,
            _ => КодыБалансовыхСчётов.КредитыФизическимЛицам.ДоВостребования
        };
    }

    private async Task<long> GetNextAccountPersonalCode(CancellationToken ct)
    {
        var sequenceNameFullQuantified = dbContext.Schema is null
            ? AccountsDbContext.AccountPersonalCodeSequenceName
            : string.Join('.', dbContext.Schema, AccountsDbContext.AccountPersonalCodeSequenceName);

        var dbConnection = dbContext.Database.GetDbConnection();
        await dbConnection.OpenAsync(ct);

        await using var dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = $"SELECT nextval('{sequenceNameFullQuantified}')";
        var value = await dbCommand.ExecuteScalarAsync(ct);

        ArgumentNullException.ThrowIfNull(value, "Запрос на получение id для счёта вернул null");
        return (long)value;
    }
}
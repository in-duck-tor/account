using InDuckTor.Account.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InDuckTor.Account.Infrastructure.Database.EntitiesConfiguration;

public class AccountNumberConverter : ValueConverter<AccountNumber, string>
{
    public AccountNumberConverter() : base(
        number => number.Value,
        stringNumber => new AccountNumber(stringNumber))
    {
    }
}
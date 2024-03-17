using InDuckTor.Account.Domain;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InDuckTor.Account.Infrastructure.Database.EntitiesConfiguration;

[UsedImplicitly]
public class AccountNumberConverter() : ValueConverter<AccountNumber, string>(
    number => number.Value,
    stringNumber => new AccountNumber(stringNumber));

[UsedImplicitly]
public class BankCodeConverter() : ValueConverter<BankCode, int>(
    number => number.Value,
    number => new BankCode(number));
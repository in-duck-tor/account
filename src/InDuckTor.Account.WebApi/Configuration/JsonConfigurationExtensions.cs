using System.Text.Json.Serialization;
using InDuckTor.Account.Domain;
using InDuckTor.Shared.Converters;
using Microsoft.AspNetCore.Http.Json;

namespace InDuckTor.Account.WebApi.Configuration;

public static class JsonConfigurationExtensions
{
    public static IServiceCollection ConfigureJsonConverters(this IServiceCollection serviceCollection) => serviceCollection.Configure<JsonOptions>(ConfigureJsonOptions);

    private static void ConfigureJsonOptions(JsonOptions options)
    {
        var jsonConverters = options.SerializerOptions.Converters;
        
        var enumMemberConverter = new JsonStringEnumMemberConverter(
            new JsonStringEnumMemberConverterOptions(),
            typeof(TransactionType), typeof(AccountState), typeof(TransactionStatus), typeof(AccountType), typeof(AccountAction));
        jsonConverters.Add(enumMemberConverter);
        
        jsonConverters.Add(new JsonConverterForParseable<AccountNumber>());
        jsonConverters.Add(new JsonConverterForParseable<BankCode>());
    }
}
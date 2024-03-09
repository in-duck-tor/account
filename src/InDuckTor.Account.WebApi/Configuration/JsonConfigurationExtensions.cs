using System.Text.Json.Serialization;
using InDuckTor.Account.Domain;
using Microsoft.AspNetCore.Http.Json;

namespace InDuckTor.Account.WebApi.Configuration;

public static class JsonConfigurationExtensions
{
    public static IServiceCollection ConfigureJsonConverters(this IServiceCollection serviceCollection) => serviceCollection.Configure<JsonOptions>(ConfigureJsonOptions);

    private static void ConfigureJsonOptions(JsonOptions options)
    {
        var enumMemberConverter = new JsonStringEnumMemberConverter(
            new JsonStringEnumMemberConverterOptions(),
            typeof(TransactionType), typeof(AccountState), typeof(TransactionType), typeof(AccountType), typeof(AccountAction));

        options.SerializerOptions.Converters.Add(enumMemberConverter);
    }
}
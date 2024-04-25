using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.KafkaClient;
using InDuckTor.Shared.Kafka;
using InDuckTor.Shared.Kafka.Interceptors;

namespace InDuckTor.Account.WebApi.Configuration;

public static class KafkaConfiguration
{
    public static IServiceCollection AddAccountsApiKafka(this IServiceCollection services, IConfiguration configuration)
    {
        var kafkaConfig = configuration.GetSection("Kafka");
        var schemaRegistries = kafkaConfig.GetSection("SchemaRegistries");
        var defaultSchemaRegistry = schemaRegistries.GetSection("Default");

        var consumers = kafkaConfig.GetSection("Consumers");
        var producers = kafkaConfig.GetSection("Producers");

        services.AddInDuckTorKafka()
            .AddProducer<AccountCommandKey, AccountCommandEnvelop>(
                configSection: producers.GetSection("AccountCommand"),
                configureBuilder: builder =>
                {
                    var schemaRegistryConfig = defaultSchemaRegistry.Get<SchemaRegistryConfig>();
                    var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);
                    builder.SetValueSerializer(new ProtobufSerializer<AccountCommandEnvelop>(schemaRegistryClient));
                    builder.SetKeySerializer(new AccountCommandKeySerializer());
                },
                addInterceptors: builder =>
                {
                    builder
                        .AddInterceptor<ConversationProducerInterceptor<AccountCommandKey, AccountCommandEnvelop>>()
                        .AddInterceptor<AccountCommandSecurityContextEnrich>();
                });

        return services;
    }
}
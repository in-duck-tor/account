using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Shared.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InDuckTor.Account.Infrastructure.Kafka;

public static class DependencyRegistration
{
    public static IServiceCollection AddAccountsKafka(this IServiceCollection services, IConfiguration configuration)
    {
        var kafkaConfig = configuration.GetSection("Kafka");
        var schemaRegistries = kafkaConfig.GetSection("SchemaRegistries");
        var defaultSchemaRegistry = schemaRegistries.GetSection("Default");
        
        var consumers = kafkaConfig.GetSection("Consumers");
        var producers = kafkaConfig.GetSection("Producers");

        
        services.AddInDuckTorKafka()
            .AddProducer<Null, AccountEnvelop>(
                producers.GetSection("Account"),
                config =>
                {
                    return;
                },
                builder =>
                {
                    var schemaRegistryConfig = defaultSchemaRegistry.Get<SchemaRegistryConfig>();
                    var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);
                    builder.SetValueSerializer(new ProtobufSerializer<AccountEnvelop>(schemaRegistryClient).AsSyncOverAsync());
                })
            .AddProducer<string, TransactionEnvelop>(
                producers.GetSection("Transaction"),
                config =>
                {
                    return;
                },
                builder =>
                {
                    var schemaRegistryConfig = defaultSchemaRegistry.Get<SchemaRegistryConfig>();
                    var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);
                    builder.SetValueSerializer(new ProtobufSerializer<TransactionEnvelop>(schemaRegistryClient).AsSyncOverAsync());
                });

        return services;
    }
}
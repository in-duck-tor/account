using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Worker.Consumers;
using InDuckTor.Shared.Kafka;
using InDuckTor.Shared.Kafka.Interceptors;
using InDuckTor.Shared.Protobuf;

namespace InDuckTor.Account.Worker.Configuration;

public static class KafkaConfiguration
{
    public static IServiceCollection AddWorkerKafka(this IServiceCollection services, IConfiguration configuration)
    {
        var kafkaConfig = configuration.GetSection("Kafka");
        var schemaRegistries = kafkaConfig.GetSection("SchemaRegistries");
        var defaultSchemaRegistry = schemaRegistries.GetSection("Default");

        var consumers = kafkaConfig.GetSection("Consumers");
        var producers = kafkaConfig.GetSection("Producers");


        services.AddInDuckTorKafka()
            .AddConsumer<AccountCommandsConsumer, string, AccountCommandEnvelop>(
                configSection: consumers.GetSection("AccountCommand"),
                configureBuilder: builder =>
                {
                    builder.SetValueDeserializer(new ProtobufDeserializer<AccountCommandEnvelop>().AsSyncOverAsync());
                    ;
                }
            )
            .AddProducer<string, CommandHandlingProblemDetails>(
                configSection: producers.GetSection("AccountCommandFail"),
                configureBuilder: builder =>
                {
                    var schemaRegistryConfig = defaultSchemaRegistry.Get<SchemaRegistryConfig>();
                    var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);
                    builder.SetValueSerializer(new ProtobufSerializer<CommandHandlingProblemDetails>(schemaRegistryClient));
                },
                addInterceptors: builder =>
                {
                    builder.AddInterceptor<ConversationProducerInterceptor<string, CommandHandlingProblemDetails>>();
                    ;
                })
            .AddProducer<string, AccountEnvelop>(
                configSection: producers.GetSection("Account"),
                configureBuilder: builder =>
                {
                    var schemaRegistryConfig = defaultSchemaRegistry.Get<SchemaRegistryConfig>();
                    var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);
                    builder.SetValueSerializer(new ProtobufSerializer<AccountEnvelop>(schemaRegistryClient));
                },
                addInterceptors: builder =>
                {
                    builder.AddInterceptor<ConversationProducerInterceptor<string, AccountEnvelop>>();
                    ;
                })
            .AddProducer<long, TransactionEnvelop>(
                configSection: producers.GetSection("Transaction"),
                configureBuilder: builder =>
                {
                    var schemaRegistryConfig = defaultSchemaRegistry.Get<SchemaRegistryConfig>();
                    var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);
                    builder.SetValueSerializer(new ProtobufSerializer<TransactionEnvelop>(schemaRegistryClient));
                },
                addInterceptors: builder =>
                {
                    builder.AddInterceptor<ConversationProducerInterceptor<long, TransactionEnvelop>>();
                    ;
                });

        return services;
    }
}
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.KafkaClient;
using InDuckTor.Shared.Kafka;
using InDuckTor.Shared.Kafka.Interceptors;
using InDuckTor.Shared.Protobuf;

namespace InDuckTor.Account.WebApi.Configuration;

public static class KafkaConfiguration
{
    public static IServiceCollection AddWebApiKafka(this IServiceCollection services, IConfiguration configuration)
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
                })

            // Легаси поддержка для v1 комманд
            // todo : remove  
            .AddNullProducer<string, CommandHandlingProblemDetails>()
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
using Confluent.Kafka.Extensions.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace InDuckTor.Account.Telemetry;

public static class TelemetryConfiguration
{
    public static void AddAccountTelemetry(this IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services.AddOpenTelemetry()
            .ConfigureResource(builder =>
            {
                builder.AddEnvironmentVariableDetector()
                    .AddTelemetrySdk();
            })
            .WithTracing(builder =>
            {
                builder.AddSource(TelemetryGlobals.ActivitySourceName);
                builder.AddAspNetCoreInstrumentation();
                builder.AddOtlpExporter(options =>
                {
                    options.ExportProcessorType = ExportProcessorType.Batch;
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;

                    // options.Endpoint OTEL_EXPORTER_OTLP_ENDPOINT
                });
                builder.AddConfluentKafkaInstrumentation();
            })
            .WithMetrics(builder =>
            {
                builder.AddMeter(TelemetryGlobals.MeterName);
                builder.AddOtlpExporter(options =>
                {
                    options.ExportProcessorType = ExportProcessorType.Batch;
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
            });

        // applicationBuilder.Logging.AddOpenTelemetry(loggingOptions =>
        // {
        //     loggingOptions.AddOtlpExporter();
        // });
    }
}
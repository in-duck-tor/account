using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace InDuckTor.Account.Telemetry;

public static class TelemetryGlobals
{
    public static readonly string? AssemblyVersion = typeof(TelemetryGlobals).Assembly.GetName().Version?.ToString();

    public static readonly string ActivitySourceName = string.Join('.', nameof(InDuckTor), nameof(Account));
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName, AssemblyVersion);

    public static readonly string MeterName = string.Join('.', nameof(InDuckTor), nameof(Account));
    public static readonly Meter Meter = new(MeterName, AssemblyVersion);
}
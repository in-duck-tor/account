namespace InDuckTor.Account.Worker.BackgroundJobs;

public class MaintenanceBackGroundService : BackgroundService
{
    private readonly IConfiguration _configuration;

    public MaintenanceBackGroundService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ScheduleStaticJobs();
        return Task.CompletedTask;
    }

    private void ScheduleStaticJobs()
    {
        PullCbCurrencyJob.Schedule(_configuration);
        RemoveExpiredIdempotencyRecordsJob.Schedule(_configuration);
    }
}
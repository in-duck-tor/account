namespace InDuckTor.Account.WebApi.BackgroundJobs;

public class MaintanceBackGroundService : BackgroundService
{
    private readonly IConfiguration _configuration;

    public MaintanceBackGroundService(IConfiguration configuration)
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
    }
}
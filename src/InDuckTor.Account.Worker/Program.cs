using System.Reflection;
using Hangfire;
using InDuckTor.Account.Cbr.Integration;
using InDuckTor.Account.Features.Mapping;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Account.Infrastructure.Hangfire;
using InDuckTor.Account.Telemetry;
using InDuckTor.Account.Worker.BackgroundJobs;
using InDuckTor.Account.Worker.Configuration;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddAccountsDbContext(configuration);
builder.Services.AddWorkerKafka(configuration);
builder.Services.AddAccountsHangfire(configuration);
// Add the processing server as IHostedService
builder.Services.AddHangfireServer((provider, options) =>
{
    options.WorkerCount = 1;
    ;
});
builder.Services.AddCbrIntegration();

builder.Services.AddScoped<ISecurityContext, SecurityContext>();

builder.Services.AddStrategiesFrom(
    Assembly.GetExecutingAssembly(),
    Assembly.GetAssembly(typeof(InDuckTor.Account.Features.Account.CreateAccount.ICreateAccount))!);
builder.Services.AddWorkerServices();

MapsterConfiguration.ConfigureMapster(Assembly.GetAssembly(typeof(AccountCommandsMapping))!);
builder.AddAccountTelemetry();

builder.Services.AddHostedService<MaintenanceBackGroundService>();

var host = builder.Build();
host.Run();
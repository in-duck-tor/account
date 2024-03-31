using System.Reflection;
using InDuckTor.Account.Cbr.Integration;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Account.Infrastructure.Hangfire;
using InDuckTor.Account.Infrastructure.Kafka;
using InDuckTor.Account.WebApi.BackgroundJobs;
using InDuckTor.Account.WebApi.Configuration;
using InDuckTor.Account.WebApi.Endpoints;
using InDuckTor.Shared.Security.Http;
using InDuckTor.Shared.Security.Jwt;
using InDuckTor.Shared.Strategies;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddSerilog((provider, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Services(provider)
        .WriteTo.Console(theme: builder.Environment.IsProduction() ? /* todo : json structured logs */null : AnsiConsoleTheme.Code);
});

builder.Services.AddStrategiesFrom(Assembly.GetAssembly(typeof(ICreateTransaction))!);
builder.Services.AddCbrIntegration();

builder.Services.AddInDuckTorAuthentication(configuration.GetSection(nameof(JwtSettings)));
builder.Services.AddAuthorization();
builder.Services.AddInDuckTorSecurity();

builder.Services.AddAccountsKafka(configuration);
builder.Services.AddAccountsDbContext(configuration);
builder.Services.AddAccountsHangfire(configuration);

builder.Services.AddHostedService<MaintanceBackGroundService>();

builder.Services.AddProblemDetails()
    .ConfigureJsonConverters();

// todo использовать GW 
builder.Services.AddCors(options => { options.AddDefaultPolicy(policyBuilder => { policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); }); });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAccountSwaggerGen();


var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// todo использовать GW 
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.UseInDuckTorSecurity();

app.AddPaymentAccountEndpoints()
    .AddBankingAccountEndpoints()
    .AddBankInfoEndpoints();

app.Run();
using System.Reflection;
using Confluent.Kafka;
using Google.Protobuf.WellKnownTypes;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Account.Infrastructure.Kafka;
using InDuckTor.Account.WebApi.Configuration;
using InDuckTor.Account.WebApi.Endpoints;
using InDuckTor.Shared.Kafka;
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

builder.Services.AddInDuckTorAuthentication(configuration.GetSection(nameof(JwtSettings)));
builder.Services.AddAuthorization();
builder.Services.AddInDuckTorSecurity();

builder.Services.AddAccountsKafka(configuration.GetSection("Kafka"));

builder.Services.AddAccountsDbContext(configuration);

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

// app.UseHttpsRedirection();

// todo использовать GW 
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.UseInDuckTorSecurity();

app.AddPaymentAccountEndpoints()
    .AddBankingAccountEndpoints()
    .AddBankInfoEndpoints();

Task.Run(async () =>
{
    var cash = app.Services.CreateScope().ServiceProvider.GetRequiredService<AccountsDbContext>().Accounts.First(account => account.Type == InDuckTor.Account.Domain.AccountType.CashRegister);
    Thread.Sleep(TimeSpan.FromSeconds(20));
    var producer = app.Services.GetRequiredService<ITopicProducer<Null, AccountEnvelop>>();
    await producer.Produce(null!, new AccountEnvelop
        {
            CorrelationId = Guid.NewGuid().ToString(),
            CreatedAt = cash.CreatedAt.ToTimestamp(),
            AccountCreated = new AccountCreated
            {
                Type = (AccountType)cash.Type,
                State = (AccountState)cash.State,
                AccountNumber = cash.Number,
                CurrencyCode = cash.CurrencyCode,
                GrantedUsers = { cash.GrantedUsers.Select(user => new GrantedAccountUser { Id = user.Id, Actions = { user.Actions.Select(action => (AccountAction)action) } }) },
                OwnerId = cash.OwnerId,
                CreatedById = cash.CreatedBy
            }
        },
        default);
});

app.Run();
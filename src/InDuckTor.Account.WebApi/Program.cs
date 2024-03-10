using System.Reflection;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Account.WebApi.Configuration;
using InDuckTor.Account.WebApi.Endpoints;
using InDuckTor.Shared.Security;
using InDuckTor.Shared.Security.Jwt;
using InDuckTor.Shared.Strategies;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddStrategiesFrom(Assembly.GetAssembly(typeof(ICreateTransaction))!);

builder.Services.AddInDuckTorAuthentication(configuration.GetSection(nameof(JwtSettings)));
builder.Services.AddAuthorization();
builder.Services.AddInDuckTorSecurity();

builder.Services.AddAccountsDbContext(configuration);

builder.Services.AddProblemDetails()
    .ConfigureJsonConverters();

// todo использовать GW 
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAccountSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// todo использовать GW 
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.UseInDuckTorSecurity();

app.AddPaymentAccountEndpoints()
    .AddBankingAccountEndpoints()
    .AddBankInfoEndpoints();

app.Run();
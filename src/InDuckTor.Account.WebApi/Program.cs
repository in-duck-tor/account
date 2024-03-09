using InDuckTor.Account.WebApi.Configuration;
using InDuckTor.Account.WebApi.Endpoints;
using InDuckTor.Shared.Security;
using InDuckTor.Shared.Security.Jwt;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddInDuckTorAuthentication(builder.Configuration.GetSection(nameof(JwtSettings)));
builder.Services.AddAuthorization();
builder.Services.AddInDuckTorSecurity();

builder.Services.AddProblemDetails()
    .ConfigureJsonConverters();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAccountSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.AddPaymentAccountEndpoints()
    .AddBankingAccountEndpoints()
    .AddBankInfoEndpoints();

app.Run();
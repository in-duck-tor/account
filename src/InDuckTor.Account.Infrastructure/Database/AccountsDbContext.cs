using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Account.Infrastructure.Database;

public class AccountsDbContext(DbContextOptions<AccountsDbContext> options) : DbContext(options)
{
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
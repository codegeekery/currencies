using Microsoft.EntityFrameworkCore;
using CurrenciesApi.Models;

namespace CurrenciesApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Rate> Rates => Set<Rate>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Currency>().HasKey(c => c.Code);
    }
}
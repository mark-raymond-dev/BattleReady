using BattleReady.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BattleReady.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ApiRequestLog> ApiRequestLogs { get; set; }
}

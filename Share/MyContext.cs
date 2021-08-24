using Microsoft.EntityFrameworkCore;
using Wlniao;
using Models;

public class MyContext : DbContext
{
    private static string connstr = null;
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (connstr == null)
        {
            connstr = DbConnectInfo.WLN_CONNSTR;
        }
        optionsBuilder.UseMySql(connstr, x => x.MigrationsHistoryTable("__efmigrationshistory"));
    }
    public DbSet<Auth> Auth { get; set; }
    public DbSet<Account> Account { get; set; }
    public DbSet<Organ> Organ { get; set; }
}
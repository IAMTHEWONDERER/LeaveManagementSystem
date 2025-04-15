using Microsoft.EntityFrameworkCore;
using DotNetEnv;

public class dataContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){ 
          DotNetEnv.Env.Load();
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
}

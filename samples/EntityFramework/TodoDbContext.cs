using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

public class TodoDbContext : DbContext
{
    public DbSet<TodoItem> TodoItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var loggerFactory = LoggerFactory.Create(factory => factory.AddConsole());

        optionsBuilder.UseLoggerFactory(loggerFactory);
        optionsBuilder.ReplaceService<IExecutionStrategyFactory, PollyExecutionStrategyFactory>();
        optionsBuilder.UseInMemoryDatabase("data");

        base.OnConfiguring(optionsBuilder);
    }
}

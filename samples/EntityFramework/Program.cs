using Microsoft.EntityFrameworkCore;

await using var context = new TodoDbContext();
var strategy = context.Database.CreateExecutionStrategy();

for (int i = 0; i < 10; i++)
{
    await strategy.ExecuteAsync(async () =>
    {
        if (Random.Shared.NextDouble() > 0.8)
        {
            throw new InvalidOperationException("Simulating transient error!");
        }

        context.Add(new TodoItem { Text = $"Todo Item - {i}" });
        await context.SaveChangesAsync();
    });
}

Console.WriteLine($"Items: {context.TodoItems.Count()}");

namespace Chaos;

public class TodosClient(HttpClient client)
{
    public async Task<IEnumerable<TodoModel>> GetTodosAsync(CancellationToken cancellationToken)
        => await client.GetFromJsonAsync<IEnumerable<TodoModel>>("/todos", cancellationToken) ?? [];
}

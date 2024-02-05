namespace Chaos;

public class TodosClient(HttpClient client)
{
    public async Task<IEnumerable<TodoModel>?> GetTodosAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/todos");
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<TodoModel>>(cancellationToken))!.Take(10);
    }
}
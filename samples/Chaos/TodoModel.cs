using System.Text.Json.Serialization;

namespace Chaos;

public record TodoModel(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title);
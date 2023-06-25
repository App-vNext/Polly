namespace Polly.TestUtils;

public record MeteringEvent(object Measurement, string Name, Dictionary<string, object?> Tags);

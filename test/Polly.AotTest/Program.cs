using Polly.Utils.Pipeline;

// See https://github.com/App-vNext/Polly/issues/1732#issuecomment-1782466692.
// This code is needed as a workaround until https://github.com/dotnet/runtime/issues/94131 is resolved.
var pipeline = CompositeComponent.Create(new[] { PipelineComponent.Empty, PipelineComponent.Empty }, null!, null!);
await pipeline.ExecuteCore<int, int>((state, context) => default, default!, default);

Console.WriteLine("Hello Polly!");

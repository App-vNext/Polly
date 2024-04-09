open System
open System.Threading
open System.Threading.Tasks
open Polly

let getBestFilmAsync(token: CancellationToken) =
    task {
        do! Task.Delay(1000, token) |> Async.AwaitTask
        return "https://www.imdb.com/title/tt0080684/"
    }

let demo() =
    async {
        // The ResiliencePipelineBuilder creates a ResiliencePipeline
        // that can be executed synchronously or asynchronously
        // and for both void and result-returning user-callbacks.
        let pipeline = ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromSeconds(5)).Build()

        // Synchronously
        pipeline.Execute(Action(fun () -> Console.WriteLine("Hello, world!")))

        // Asynchronously
        // Note that the function is wrapped in a ValueTask for Polly to use as F# cannot
        // await ValueTask directly, and AsTask() is used to convert the ValueTask returned by
        // ExecuteAsync() to a Task so it can be awaited.
        do! pipeline.ExecuteAsync(Func<CancellationToken, ValueTask>(fun token -> new ValueTask(task {
            Console.WriteLine("Hello, world! Waiting for 1 second...")
            do! Task.Delay(1000, token) |> Async.AwaitTask
        })),
        CancellationToken.None).AsTask() |> Async.AwaitTask

        // Synchronously with result
        let someResult = pipeline.Execute(Func<CancellationToken, string>(fun token -> "some-result"))

        // Asynchronously with result
        // Note that the function is wrapped in a ValueTask<string> for Polly to use as F# cannot
        // await ValueTask directly, and AsTask() is used to convert the ValueTask<string> returned by
        // ExecuteAsync() to a Task<string> so it can be awaited.
        let! bestFilm = pipeline.ExecuteAsync(
            Func<CancellationToken, ValueTask<string>>(fun token -> new ValueTask<string>(getBestFilmAsync(token))),
            CancellationToken.None).AsTask() |> Async.AwaitTask

        Console.WriteLine("Link to the best film: {0}", bestFilm)
    }

[<EntryPoint>]
let main _ =
    demo() |> Async.RunSynchronously
    0

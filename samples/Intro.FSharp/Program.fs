open FSharp.Control
open System
open System.Threading
open System.Threading.Tasks
open IcedTasks
open Polly

let getBestFilmAsync token =
    task {
        do! Task.Delay(1000, token)
        return "https://www.imdb.com/title/tt0080684/"
    }

let demo () =
    task {
        // The ResiliencePipelineBuilder creates a ResiliencePipeline
        // that can be executed synchronously or asynchronously
        // and for both void and result-returning user-callbacks.
        let pipeline =
            ResiliencePipelineBuilder()
                .AddTimeout(TimeSpan.FromSeconds(5))
                .Build()

        let token = CancellationToken.None

        // Synchronously
        pipeline.Execute(fun () -> printfn "Hello, world!")

        // Asynchronously
        // Note that Polly expects a ValueTask to be returned, so the function uses the valueTask builder
        // from IcedTasks to make it easier to use ValueTask. See https://github.com/TheAngryByrd/IcedTasks.
        do! pipeline.ExecuteAsync(
            fun token ->
                valueTask {
                    printfn "Hello, world! Waiting for 2 seconds..."
                    do! Task.Delay(1000, token)
                    printfn "Wait complete."
                }
            , token
        )

        // Synchronously with result
        let someResult = pipeline.Execute(fun token -> "some-result")

        // Asynchronously with result
        // Note that Polly expects a ValueTask<T> to be returned, so the function uses the valueTask builder
        // from IcedTasks to make it easier to use ValueTask<T>. See https://github.com/TheAngryByrd/IcedTasks.
        let! bestFilm = pipeline.ExecuteAsync(
            fun token ->
                valueTask {
                    let! url = getBestFilmAsync(token)
                    return url
                }
            , token
        )

        printfn $"Link to the best film: {bestFilm}"
    }

[<EntryPoint>]
let main _ =
    demo().Wait()
    0

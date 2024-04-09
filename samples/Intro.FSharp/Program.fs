open FSharp.Control
open System
open System.Threading
open System.Threading.Tasks
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
            ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromSeconds(5)).Build()

        // Synchronously
        pipeline.Execute(fun () -> printfn "Hello, world!")

        // Asynchronously
        // Note that Polly expects a ValueTask to be returned, so the function is piped into a ValueTask constructor.
        do!
            pipeline.ExecuteAsync(
                fun token ->
                    task {
                        printfn "Hello, world! Waiting for 1 second..."
                        do! Task.Delay(1000, token)
                    } |> ValueTask
                , CancellationToken.None
            )

        // Synchronously with result
        let someResult = pipeline.Execute(fun token -> "some-result")

        // Asynchronously with result
        // Note that Polly expects a ValueTask to be returned, so the function is piped into a ValueTask constructor.
        let! bestFilm =
            pipeline.ExecuteAsync((fun token -> getBestFilmAsync(token) |> ValueTask<string>), CancellationToken.None)

        printfn $"Link to the best film: {bestFilm}"
    }

[<EntryPoint>]
let main _ =
    demo().Wait()
    0

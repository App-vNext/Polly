# Use with F# and Visual Basic

Asynchronous methods in the Polly.Core API return either `ValueTask` or `ValueTask<T>`
instead of `Task` or `Task<T>`. This is because Polly v8 was designed to be optimized
for high performance and uses `ValueTask` to avoid unnecessary allocations.

One downside to this choice is that in Visual Basic and F#, it is not possible to directly
await a method that returns `ValueTask` or `ValueTask<T>`, instead requiring the use of
`Task` and `Task<T>`.

A proposal to support awaiting `ValueTask` can be found in F# language design repository:
[[RFC FS-1021 Discussion] Support Interop with ValueTask in Async Type][fsharp-fslang-design-118].

To work around this limitation, you can use the [`AsTask()`][valuetask-astask] method to convert a
`ValueTask` to a `Task` in F# and Visual Basic. This does however introduce an allocation and make
the code a bit more difficult to work with compared to C#.

Examples of such conversions are shown below.

## F\#

```fsharp
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
```

[Source][sample-fsharp]

## Visual Basic

```vb
Imports System.Threading
Imports Polly

Module Program
    Sub Main()
        Demo().Wait()
    End Sub

    Async Function Demo() As Task
        ' The ResiliencePipelineBuilder creates a ResiliencePipeline
        ' that can be executed synchronously or asynchronously
        ' and for both void and result-returning user-callbacks.
        Dim pipeline = New ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromSeconds(5)).Build()

        ' Synchronously
        pipeline.Execute(Sub()
                             Console.WriteLine("Hello, world!")
                         End Sub)

        ' Asynchronously
        ' Note that the function is wrapped in a ValueTask for Polly to use as VB.NET cannot
        ' await ValueTask directly, and AsTask() is used to convert the ValueTask returned by
        ' ExecuteAsync() to a Task so it can be awaited.
        Await pipeline.ExecuteAsync(Function(token)
                                        Return New ValueTask(GreetAndWaitAsync(token))
                                    End Function,
                                    CancellationToken.None).AsTask()

        ' Synchronously with result
        Dim someResult = pipeline.Execute(Function(token)
                                              Return "some-result"
                                          End Function)

        ' Asynchronously with result
        ' Note that the function is wrapped in a ValueTask(Of String) for Polly to use as VB.NET cannot
        ' await ValueTask directly, and AsTask() is used to convert the ValueTask(Of String) returned by
        ' ExecuteAsync() to a Task(Of String) so it can be awaited.
        Dim bestFilm = Await pipeline.ExecuteAsync(Function(token)
                                                       Return New ValueTask(Of String)(GetBestFilmAsync(token))
                                                   End Function,
                                    CancellationToken.None).AsTask()

        Console.WriteLine("Link to the best film: {0}", bestFilm)

    End Function

    Async Function GreetAndWaitAsync(token As CancellationToken) As Task
        Console.WriteLine("Hello, world! Waiting for 1 second...")
        Await Task.Delay(1000, token)
    End Function

    Async Function GetBestFilmAsync(token As CancellationToken) As Task(Of String)
        Await Task.Delay(1000, token)
        Return "https://www.imdb.com/title/tt0080684/"
    End Function
End Module
```

[Source][sample-vb]

[fsharp-fslang-design-118]: https://github.com/fsharp/fslang-design/discussions/118
[valuetask-astask]: https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask.astask
[sample-fsharp]: https://github.com/App-vNext/Polly/tree/main/samples/Intro.FSharp
[sample-vb]: https://github.com/App-vNext/Polly/tree/main/samples/Intro.VisualBasic

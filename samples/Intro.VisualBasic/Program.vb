Imports System.Threading
Imports Polly

Module Progam
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

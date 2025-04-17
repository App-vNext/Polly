# HTTP client integration samples

Transient failures are inevitable for HTTP based communication as well. It is not a surprise that many developers use Polly with an HTTP client to make there applications more robust.

Here we have collected some popular HTTP client libraries and show how to integrate them with Polly.

## Setting the stage

In the examples below we will register HTTP clients into a Dependency Injection container.

The same resilience strategy will be used each time to keep the samples focused on the HTTP client library integration.

<!-- snippet: http-client-integrations-handle-transient-errors -->
```cs
private static ValueTask<bool> HandleTransientHttpError(Outcome<HttpResponseMessage> outcome) => outcome switch
{
    { Exception: HttpRequestException } => PredicateResult.True(),
    { Result.StatusCode: HttpStatusCode.RequestTimeout } => PredicateResult.True(),
    { Result.StatusCode: >= HttpStatusCode.InternalServerError } => PredicateResult.True(),
    _ => PredicateResult.False()
};

private static RetryStrategyOptions<HttpResponseMessage> GetRetryOptions() =>
new()
{
    ShouldHandle = args => HandleTransientHttpError(args.Outcome),
    MaxRetryAttempts = 3,
    BackoffType = DelayBackoffType.Exponential,
    Delay = TimeSpan.FromSeconds(2)
};
```
<!-- endSnippet -->

Here we create a strategy which will retry the HTTP request if the status code is either `408`, greater than or equal to `500`, or an `HttpRequestException` is thrown.

The `HandleTransientHttpError` method is equivalent to the [`HttpPolicyExtensions.HandleTransientHttpError`](https://github.com/App-vNext/Polly.Extensions.Http/blob/93b91c4359f436bda37f870c4453f25555b9bfd8/src/Polly.Extensions.Http/HttpPolicyExtensions.cs) method in the [App-vNext/Polly.Extensions.Http](https://github.com/App-vNext/Polly.Extensions.Http) repository.

## With HttpClient

We use the [`AddResilienceHandler`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.resiliencehttpclientbuilderextensions.addresiliencehandler) method to register our resilience strategy with the built-in [`HttpClient`](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient).

<!-- snippet: http-client-integrations-httpclient -->
```cs
var services = new ServiceCollection();

// Register a named HttpClient and decorate with a resilience pipeline
services.AddHttpClient(HttpClientName)
        .ConfigureHttpClient(client => client.BaseAddress = BaseAddress)
        .AddResilienceHandler("httpclient_based_pipeline",
            builder => builder.AddRetry(GetRetryOptions()));

using var provider = services.BuildServiceProvider();

// Resolve the named HttpClient
var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
var httpClient = httpClientFactory.CreateClient(HttpClientName);

// Use the HttpClient by making a request
var response = await httpClient.GetAsync("/408");
```
<!-- endSnippet -->

> [!NOTE]
> The following packages are required to the above example:
>
> - [Microsoft.Extensions.DependencyInjection][m.e.dependencyinjection]: Required for the dependency injection functionality
> - [Microsoft.Extensions.Http.Resilience][m.e.http.resilience]: Required for the `AddResilienceHandler` extension

### Further reading for HttpClient

- [Build resilient HTTP apps: Key development patterns](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
- [Building resilient cloud services with .NET 8](https://devblogs.microsoft.com/dotnet/building-resilient-cloud-services-with-dotnet-8/)

## With Flurl

[Flurl][flurl] is a URL builder and HTTP client library for .NET.

The named `HttpClient` registration and its decoration with our resilience strategy are the same as the built-in `HttpClient`.

Here we create a `FlurlClient` which uses the decorated, named `HttpClient` for HTTP requests.

<!-- snippet: http-client-integrations-flurl -->
```cs
var services = new ServiceCollection();

// Register a named HttpClient and decorate with a resilience pipeline
services.AddHttpClient(HttpClientName)
        .ConfigureHttpClient(client => client.BaseAddress = BaseAddress)
        .AddResilienceHandler("flurl_based_pipeline",
            builder => builder.AddRetry(GetRetryOptions()));

using var provider = services.BuildServiceProvider();

// Resolve the named HttpClient and create a new FlurlClient
var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
var flurlClient = new FlurlClient(httpClientFactory.CreateClient(HttpClientName));

// Use the FlurlClient by making a request
var response = await flurlClient.Request("/408").GetAsync();
```
<!-- endSnippet -->

> [!NOTE]
> The following packages are required to the above example:
>
> - [Microsoft.Extensions.DependencyInjection][m.e.dependencyinjection]: Required for the dependency injection functionality
> - [Microsoft.Extensions.Http.Resilience][m.e.http.resilience]: Required for the `AddResilienceHandler` extension
> - [Flurl.Http](https://www.nuget.org/packages/Flurl.Http/): Required for the `FlurlClient`

### Further reading for Flurl

- [Flurl home page][flurl]

## With Refit

[Refit](https://github.com/reactiveui/refit) is an automatic type-safe REST library for .NET.

First let's define the API interface:

<!-- snippet: http-client-integrations-refit-interface -->
```cs
public interface IHttpStatusApi
{
    [Get("/408")]
    Task<HttpResponseMessage> GetRequestTimeoutEndpointAsync();
}
```
<!-- endSnippet -->

Then use the `AddRefitClient` method to register the interface as a typed `HttpClient`. Finally we call `AddResilienceHandler` to decorate the underlying `HttpClient` with our resilience strategy.

<!-- snippet: http-client-integrations-refit -->
```cs
var services = new ServiceCollection();

// Register a Refit generated typed HttpClient and decorate with a resilience pipeline
services.AddRefitClient<IHttpStatusApi>()
        .ConfigureHttpClient(client => client.BaseAddress = BaseAddress)
        .AddResilienceHandler("refit_based_pipeline",
            builder => builder.AddRetry(GetRetryOptions()));

// Resolve the typed HttpClient
using var provider = services.BuildServiceProvider();
var apiClient = provider.GetRequiredService<IHttpStatusApi>();

// Use the Refit generated typed HttpClient by making a request
var response = await apiClient.GetRequestTimeoutEndpointAsync();
```
<!-- endSnippet -->

> [!NOTE]
> The following packages are required to the above example:
>
> - [Microsoft.Extensions.DependencyInjection][m.e.dependencyinjection]: Required for the dependency injection functionality
> - [Refit.HttpClientFactory](https://www.nuget.org/packages/Refit.HttpClientFactory): Required for the `AddRefitClient` extension

### Further readings for Refit

- [Using ASP.NET Core 2.1's HttpClientFactory with Refit's REST library](https://www.hanselman.com/blog/using-aspnet-core-21s-httpclientfactory-with-refits-rest-library)
- [Refit in .NET: Building Robust API Clients in C#](https://www.milanjovanovic.tech/blog/refit-in-dotnet-building-robust-api-clients-in-csharp)
- [Understand Refit in .NET Core](https://medium.com/@jaimin_99136/understand-the-refit-in-net-core-ba0097c5e620)

## With RestSharp

[RestSharp][restsharp] is a simple REST and HTTP API Client for .NET.

The named `HttpClient` registration and its decoration with our resilience strategy are the same as the built-in `HttpClient`.

Here we create a `RestClient` which uses the decorated, named `HttpClient` for HTTP requests.

<!-- snippet: http-client-integrations-restsharp -->
```cs
var services = new ServiceCollection();

// Register a named HttpClient and decorate with a resilience pipeline
services.AddHttpClient(HttpClientName)
        .ConfigureHttpClient(client => client.BaseAddress = BaseAddress)
        .AddResilienceHandler("restsharp_based_pipeline",
            builder => builder.AddRetry(GetRetryOptions()));

using var provider = services.BuildServiceProvider();

// Resolve the named HttpClient and create a RestClient
var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
var restClient = new RestClient(httpClientFactory.CreateClient(HttpClientName));

// Use the RestClient by making a request
var request = new RestRequest("/408", Method.Get);
var response = await restClient.ExecuteAsync(request);
```
<!-- endSnippet -->

> [!NOTE]
> The following packages are required to the above example:
>
> - [Microsoft.Extensions.DependencyInjection][m.e.dependencyinjection]: Required for the dependency injection functionality
> - [Microsoft.Extensions.Http.Resilience][m.e.http.resilience]: Required for the `AddResilienceHandler` extension
> - [RestSharp](https://www.nuget.org/packages/RestSharp): Required for the `RestClient`, `RestRequest`, `RestResponse`, etc. types

### Further reading for RestSharp

- [RestSharp home page][restsharp]

[flurl]: https://flurl.dev/
[m.e.dependencyinjection]: https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection
[m.e.http.resilience]: https://www.nuget.org/packages/Microsoft.Extensions.Http.Resilience
[restsharp]: https://restsharp.dev/

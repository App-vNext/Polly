# HTTP client integration samples

The transient failures are inevitable for HTTP based communication as well. It is not a surprise that many developers want to use Polly with some HTTP client.

Here we have collected some of the most commonly used HTTP client libraries and how to integrate them with Polly.

## Setting the stage

In the examples below we will register HTTP clients into a Dependency Injection container.

Each time the same resilience strategy will be used to keep the samples focused on the HTTP client library integration.

<!-- snippet: http-client-integrations-handle-transient-errors -->
```cs
private static ValueTask<bool> HandleTransientHttpError(Outcome<HttpResponseMessage> outcome)
=> outcome switch
{
    { Exception: HttpRequestException } => PredicateResult.True(),
    { Result.StatusCode: HttpStatusCode.RequestTimeout } => PredicateResult.True(),
    { Result.StatusCode: >= HttpStatusCode.InternalServerError } => PredicateResult.True(),
    _ => PredicateResult.False()
};

private static RetryStrategyOptions<HttpResponseMessage> GetRetryOptions()
=> new()
{
    ShouldHandle = args => HandleTransientHttpError(args.Outcome),
    MaxRetryAttempts = 3,
    BackoffType = DelayBackoffType.Exponential,
    Delay = TimeSpan.FromSeconds(2)
};
```
<!-- endSnippet -->

Here we create a strategy which will retry the HTTP request if the status code is either 408 or greater than 500 or an `HttpRequestException` was thrown.

The `HandleTransientHttpError` is a V8 port of the [`HttpPolicyExtensions.HandleTransientHttpError`](https://github.com/App-vNext/Polly.Extensions.Http/tree/master).

## HttpClient based

We use the [`AddResilienceHandler`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.resiliencehttpclientbuilderextensions.addresiliencehandler) method to register our resilience strategy on the built-in [`HttpClient`](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient).

<!-- snippet: http-client-integrations-httpclient -->
```cs
ServiceCollection services = new();

// Register a named HttpClient and decorate with a resilience pipeline
services.AddHttpClient(string.Empty)
        .ConfigureHttpClient(client => client.BaseAddress = DownstreamUri)
        .AddResilienceHandler("httpclient_based_pipeline",
            builder => builder.AddRetry(GetRetryOptions()));

var provider = services.BuildServiceProvider();

// Resolve the named HttpClient
var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
var httpClient = httpClientFactory.CreateClient();

// Use the HttpClient by making a request
var response = await httpClient.GetAsync(new Uri("/408"));
```
<!-- endSnippet -->

> [!NOTE]
> The following packages are required to the above example:
>
> - [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/microsoft.extensions.dependencyinjection): Required for the dependency injection structures
> - [Microsoft.Extensions.Http](https://www.nuget.org/packages/Microsoft.Extensions.Http/): Required for the `AddHttpClient` extension
> - [Microsoft.Extensions.Http.Resilience](https://www.nuget.org/packages/Microsoft.Extensions.Http.Resilience): Required for the `AddResilienceHandler` extension

### Further readings for HttpClient

- [Build resilient HTTP apps: Key development patterns](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)
- [Building resilient cloud services with .NET 8](https://devblogs.microsoft.com/dotnet/building-resilient-cloud-services-with-dotnet-8/)

## Flurl based

The named `HttpClient` registration and its decoration with our resilience strategy are the same as the built-in `HttpClient`.

Here we create a `FlurlClient` which uses the decorated, named `HttpClient` to perform HTTP communication.

<!-- snippet: http-client-integrations-flurl -->
```cs
ServiceCollection services = new();

// Register a named HttpClient and decorate with a resilience pipeline
services.AddHttpClient(string.Empty)
        .ConfigureHttpClient(client => client.BaseAddress = DownstreamUri)
        .AddResilienceHandler("flurl_based_pipeline",
            builder => builder.AddRetry(GetRetryOptions()));

var provider = services.BuildServiceProvider();

// Resolve the named HttpClient and create a new FlurlClient
var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
var apiClient = new FlurlClient(httpClientFactory.CreateClient());

// Use the FlurlClient by making a request
var res = await apiClient.Request("/408").GetAsync();
```
<!-- endSnippet -->

> [!NOTE]
> The following packages are required to the above example:
>
> - [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/microsoft.extensions.dependencyinjection): Required for the dependency injection structures
> - [Microsoft.Extensions.Http](https://www.nuget.org/packages/Microsoft.Extensions.Http/): Required for the `AddHttpClient` extension
> - [Microsoft.Extensions.Http.Resilience](https://www.nuget.org/packages/Microsoft.Extensions.Http.Resilience): Required for the `AddResilienceHandler` extension
> - [Flurl.Http](https://www.nuget.org/packages/Flurl.Http/): Required for the `FlurlClient`

### Further readings for Flurl

- [Flurl home page](https://flurl.dev/)

## Refit based

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

Then use the `AddRefitClient` to register the interface as typed HttpClient. Finally call `AddResilienceHandler` to decorate the underlying `HttpClient` with our resilience strategy.

<!-- snippet: http-client-integrations-refit -->
```cs
ServiceCollection services = new();

// Register a refit generated typed HttpClient and decorate with a resilience pipeline
services.AddRefitClient<IHttpStatusApi>()
        .ConfigureHttpClient(client => client.BaseAddress = DownstreamUri)
        .AddResilienceHandler("refit_based_pipeline",
            builder => builder.AddRetry(GetRetryOptions()));

// Resolve the typed HttpClient
var provider = services.BuildServiceProvider();
var apiClient = provider.GetRequiredService<IHttpStatusApi>();

// Use the refit generated typed HttpClient by making a request
var response = await apiClient.GetRequestTimeoutEndpointAsync();
```
<!-- endSnippet -->

> [!NOTE]
> The following packages are required to the above example:
>
> - [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/microsoft.extensions.dependencyinjection): Required for the dependency injection structures
> - [Microsoft.Extensions.Http.Resilience](https://www.nuget.org/packages/Microsoft.Extensions.Http.Resilience): Required for the `AddResilienceHandler` extension
> - [Refit.HttpClientFactory](https://www.nuget.org/packages/Refit.HttpClientFactory): Required for the `AddRefitClient` extension

### Further readings for Refit

- [Using ASP.NET Core 2.1's HttpClientFactory with Refit's REST library](https://www.hanselman.com/blog/using-aspnet-core-21s-httpclientfactory-with-refits-rest-library)
- [Refit in .NET: Building Robust API Clients in C#](https://www.milanjovanovic.tech/blog/refit-in-dotnet-building-robust-api-clients-in-csharp)
- [Understand the Refit in .NET Core](https://medium.com/@jaimin_99136/understand-the-refit-in-net-core-ba0097c5e620)

## RestSharp based

The named `HttpClient` registration and its decoration with our resilience strategy are the same as the built-in `HttpClient`.

Here we create a `RestClient` which uses the decorated, named `HttpClient` to perform HTTP communication.

<!-- snippet: http-client-integrations-restsharp -->
```cs
ServiceCollection services = new();

// Register a named HttpClient and decorate with a resilience pipeline
services.AddHttpClient(string.Empty)
        .ConfigureHttpClient(client => client.BaseAddress = DownstreamUri)
        .AddResilienceHandler("restsharp_based_pipeline",
            builder => builder.AddRetry(GetRetryOptions()));

var provider = services.BuildServiceProvider();

// Resolve the named HttpClient and create a RestClient
var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
var restClient = new RestClient(httpClientFactory.CreateClient());

// Use the RestClient by making a request
var request = new RestRequest("/408", Method.Get);
var response = await restClient.ExecuteAsync(request);
```
<!-- endSnippet -->

> [!NOTE]
> The following packages are required to the above example:
>
> - [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/microsoft.extensions.dependencyinjection): Required for the dependency injection structures
> - [Microsoft.Extensions.Http.Resilience](https://www.nuget.org/packages/Microsoft.Extensions.Http.Resilience): Required for the `AddResilienceHandler` extension
> - [RestSharp](https://www.nuget.org/packages/RestSharp): Required for the `RestClient`, `RestRequest`, `RestResponse`, etc. structures

### Further readings for RestSharp

- [RestSharp home page](https://restsharp.dev/)

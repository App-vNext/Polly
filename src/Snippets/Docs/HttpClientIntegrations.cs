using System.Net;
using System.Net.Http;
using Flurl.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly.Retry;
using Refit;
using RestSharp;

namespace Snippets.Docs;

internal static class HttpClientIntegrations
{
    private const string HttpClientName = "httpclient";
    private static readonly Uri BaseAddress = new("https://httpstat.us/");

    #region http-client-integrations-handle-transient-errors
    private static ValueTask<bool> HandleTransientHttpError(Outcome<HttpResponseMessage> outcome) =>
    outcome switch
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
    #endregion

#pragma warning disable CA2234
    public static async Task HttpClientExample()
    {
        #region http-client-integrations-httpclient
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
        #endregion
    }
#pragma warning restore CA2234

    public static async Task RefitExample()
    {
        #region http-client-integrations-refit
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
        #endregion
    }

    public static async Task FlurlExample()
    {
        #region http-client-integrations-flurl
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
        #endregion
    }

    public static async Task RestSharpExample()
    {
        #region http-client-integrations-restsharp
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
        #endregion
    }
}

#region http-client-integrations-refit-interface
public interface IHttpStatusApi
{
    [Get("/408")]
    Task<HttpResponseMessage> GetRequestTimeoutEndpointAsync();
}
#endregion

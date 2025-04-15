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
    private static readonly Uri DownstreamUri = new("https://httpstat.us/408");

    #region http-client-integrations-handle-transient-errors
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
    #endregion

    public static async Task HttpClientExample()
    {
        #region http-client-integrations-httpclient
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
        #endregion
    }

    public static async Task RefitExample()
    {
        #region http-client-integrations-refit
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
        #endregion
    }

    public static async Task FlurlExample()
    {
        #region http-client-integrations-flurl
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
        #endregion
    }

    public static async Task RestSharpExample()
    {
        #region http-client-integrations-restsharp
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

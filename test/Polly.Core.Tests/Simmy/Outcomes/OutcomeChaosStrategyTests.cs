using Moq;
using Polly.Simmy.Outcomes;
using Polly.Telemetry;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OutcomeChaosStrategyTests
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly Mock<DiagnosticSource> _diagnosticSource = new();

    public OutcomeChaosStrategyTests() => _telemetry = TestUtilities.CreateResilienceTelemetry(_diagnosticSource.Object);

    public static List<object[]> FaultCtorTestCases =>
    new()
    {
            new object[] { null!, "Value cannot be null. (Parameter 'options')" },
            new object[] { new OutcomeStrategyOptions<Exception>
            {
                InjectionRate = 1,
                Enabled = true,
            }, "Either Outcome or OutcomeGenerator is required. (Parameter 'Outcome')" },
    };

    [Theory]
    [MemberData(nameof(FaultCtorTestCases))]
    public void FaultInvalidCtor(OutcomeStrategyOptions<Exception> options, string message)
    {
        Action act = () =>
        {
            var _ = new OutcomeChaosStrategy(options, _telemetry);
        };

#if NET481
act.Should()
            .Throw<ArgumentNullException>();
#else
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage(message);
#endif
    }

    [Fact]
    public void Given_not_enabled_should_not_inject_fault()
    {
        var userDelegateExecuted = false;
        var fault = new InvalidOperationException("Dummy exception");

        var options = new OutcomeStrategyOptions<Exception>
        {
            InjectionRate = 0.6,
            Enabled = false,
            Randomizer = () => 0.5,
            Outcome = new Outcome<Exception>(fault)
        };

        var sut = CreateSut(options);
        sut.Execute(() => { userDelegateExecuted = true; });

        userDelegateExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_should_inject_fault()
    {
        var userDelegateExecuted = false;
        var exceptionMessage = "Dummy exception";
        var fault = new InvalidOperationException(exceptionMessage);

        var options = new OutcomeStrategyOptions<Exception>
        {
            InjectionRate = 0.6,
            Enabled = true,
            Randomizer = () => 0.5,
            Outcome = new Outcome<Exception>(fault)
        };

        var sut = CreateSut<int>(options);
        await sut.Invoking(s => s.ExecuteAsync(async _ =>
        {
            userDelegateExecuted = true;
            return await Task.FromResult(new Outcome<int>(200));
        }).AsTask())
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(exceptionMessage);

        userDelegateExecuted.Should().BeFalse();
    }

    [Fact]
    public void Given_enabled_and_randomly_not_within_threshold_should_not_inject_fault()
    {
        var userDelegateExecuted = false;
        var fault = new InvalidOperationException("Dummy exception");

        var options = new OutcomeStrategyOptions<Exception>
        {
            InjectionRate = 0.3,
            Enabled = true,
            Randomizer = () => 0.5,
            Outcome = new Outcome<Exception>(fault)
        };

        var sut = CreateSut<int>(options);
        var result = sut.Execute(_ =>
        {
            userDelegateExecuted = true;
            return 200;
        });

        result.Should().Be(200);
        userDelegateExecuted.Should().BeTrue();
    }

    [Fact]
    public void Given_enabled_and_randomly_within_threshold_should_not_inject_fault_when_exception_is_null()
    {
        var userDelegateExecuted = false;
        var options = new OutcomeStrategyOptions<Exception>
        {
            InjectionRate = 0.6,
            Enabled = true,
            Randomizer = () => 0.5,
            OutcomeGenerator = (_) => new ValueTask<Outcome<Exception>>(Task.FromResult(new Outcome<Exception>(null!)))
        };

        var sut = CreateSut(options);
        sut.Execute(_ =>
        {
            userDelegateExecuted = true;
        });

        userDelegateExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Should_not_inject_fault_when_it_was_cancelled_running_the_fault_generator()
    {
        var userDelegateExecuted = false;
        var fault = new InvalidOperationException("Dummy exception");

        using var cts = new CancellationTokenSource();
        var options = new OutcomeStrategyOptions<Exception>
        {
            InjectionRate = 0.6,
            Enabled = true,
            Randomizer = () => 0.5,
            OutcomeGenerator = (_) =>
            {
                cts.Cancel();
                return new ValueTask<Outcome<Exception>>(new Outcome<Exception>(fault));
            }
        };

        var sut = CreateSut<int>(options);
        var restult = await sut.ExecuteAsync(async _ =>
        {
            userDelegateExecuted = true;
            return await Task.FromResult(200);
        }, cts.Token);

        restult.Should().Be(200);
        userDelegateExecuted.Should().BeTrue();
    }

    [Fact]
    public void Given_not_enabled_should_not_inject_result()
    {
        var userDelegateExecuted = false;
        var fakeResult = HttpStatusCode.TooManyRequests;

        var options = new OutcomeStrategyOptions<HttpStatusCode>
        {
            InjectionRate = 0.6,
            Enabled = false,
            Randomizer = () => 0.5,
            Outcome = new Outcome<HttpStatusCode>(fakeResult)
        };

        var sut = CreateSut(options);
        var response = sut.Execute(() => { userDelegateExecuted = true; return HttpStatusCode.OK; });

        response.Should().Be(HttpStatusCode.OK);
        userDelegateExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_should_inject_result()
    {
        var userDelegateExecuted = false;
        var fakeResult = HttpStatusCode.TooManyRequests;

        var options = new OutcomeStrategyOptions<HttpStatusCode>
        {
            InjectionRate = 0.6,
            Enabled = true,
            Randomizer = () => 0.5,
            Outcome = new Outcome<HttpStatusCode>(fakeResult)
        };

        var sut = CreateSut(options);
        var response = await sut.ExecuteAsync(async _ =>
        {
            userDelegateExecuted = true;
            return await Task.FromResult(HttpStatusCode.OK);
        });

        response.Should().Be(fakeResult);
        userDelegateExecuted.Should().BeFalse();
    }

    [Fact]
    public void Given_enabled_and_randomly_not_within_threshold_should_not_inject_result()
    {
        var userDelegateExecuted = false;
        var fakeResult = HttpStatusCode.TooManyRequests;

        var options = new OutcomeStrategyOptions<HttpStatusCode>
        {
            InjectionRate = 0.3,
            Enabled = false,
            Randomizer = () => 0.5,
            Outcome = new Outcome<HttpStatusCode>(fakeResult)
        };

        var sut = CreateSut(options);
        var response = sut.Execute(_ =>
        {
            userDelegateExecuted = true;
            return HttpStatusCode.OK;
        });

        response.Should().Be(HttpStatusCode.OK);
        userDelegateExecuted.Should().BeTrue();
    }

    private OutcomeChaosStrategy<TResult> CreateSut<TResult>(OutcomeStrategyOptions<TResult> options) =>
        new(options, _telemetry);

    private OutcomeChaosStrategy<TResult> CreateSut<TResult>(OutcomeStrategyOptions<Exception> options) =>
        new(options, _telemetry);

    private OutcomeChaosStrategy CreateSut(OutcomeStrategyOptions<Exception> options) =>
        new(options, _telemetry);
}

/// <summary>
/// Borrowing this from the actual dotnet standard implementation since it is not available in the net481.
/// </summary>
public enum HttpStatusCode
{
    // Summary:
    //     Equivalent to HTTP status 100. System.Net.HttpStatusCode.Continue indicates that
    //     the client can continue with its request.
    Continue = 100,

    // Summary:
    //     Equivalent to HTTP status 101. System.Net.HttpStatusCode.SwitchingProtocols indicates
    //     that the protocol version or protocol is being changed.
    SwitchingProtocols = 101,

    // Summary:
    //     Equivalent to HTTP status 102. System.Net.HttpStatusCode.Processing indicates
    //     that the server has accepted the complete request but hasn't completed it yet.
    Processing = 102,

    // Summary:
    //     Equivalent to HTTP status 103. System.Net.HttpStatusCode.EarlyHints indicates
    //     to the client that the server is likely to send a final response with the header
    //     fields included in the informational response.
    EarlyHints = 103,

    // Summary:
    //     Equivalent to HTTP status 200. System.Net.HttpStatusCode.OK indicates that the
    //     request succeeded and that the requested information is in the response. This
    //     is the most common status code to receive.
    OK = 200,

    // Summary:
    //     Equivalent to HTTP status 201. System.Net.HttpStatusCode.Created indicates that
    //     the request resulted in a new resource created before the response was sent.
    Created = 201,

    // Summary:
    //     Equivalent to HTTP status 202. System.Net.HttpStatusCode.Accepted indicates that
    //     the request has been accepted for further processing.
    Accepted = 202,

    // Summary:
    //     Equivalent to HTTP status 203. System.Net.HttpStatusCode.NonAuthoritativeInformation
    //     indicates that the returned meta information is from a cached copy instead of
    //     the origin server and therefore may be incorrect.
    NonAuthoritativeInformation = 203,

    // Summary:
    //     Equivalent to HTTP status 204. System.Net.HttpStatusCode.NoContent indicates
    //     that the request has been successfully processed and that the response is intentionally
    //     blank.
    NoContent = 204,

    // Summary:
    //     Equivalent to HTTP status 205. System.Net.HttpStatusCode.ResetContent indicates
    //     that the client should reset (not reload) the current resource.
    ResetContent = 205,

    // Summary:
    //     Equivalent to HTTP status 206. System.Net.HttpStatusCode.PartialContent indicates
    //     that the response is a partial response as requested by a GET request that includes
    //     a byte range.
    PartialContent = 206,

    // Summary:
    //     Equivalent to HTTP status 207. System.Net.HttpStatusCode.MultiStatus indicates
    //     multiple status codes for a single response during a Web Distributed Authoring
    //     and Versioning (WebDAV) operation. The response body contains XML that describes
    //     the status codes.
    MultiStatus = 207,

    // Summary:
    //     Equivalent to HTTP status 208. System.Net.HttpStatusCode.AlreadyReported indicates
    //     that the members of a WebDAV binding have already been enumerated in a preceding
    //     part of the multistatus response, and are not being included again.
    AlreadyReported = 208,

    // Summary:
    //     Equivalent to HTTP status 226. System.Net.HttpStatusCode.IMUsed indicates that
    //     the server has fulfilled a request for the resource, and the response is a representation
    //     of the result of one or more instance-manipulations applied to the current instance.
    IMUsed = 226,

    // Summary:
    //     Equivalent to HTTP status 300. System.Net.HttpStatusCode.Ambiguous indicates
    //     that the requested information has multiple representations. The default action
    //     is to treat this status as a redirect and follow the contents of the Location
    //     header associated with this response. Ambiguous is a synonym for MultipleChoices.
    Ambiguous = 300,

    // Summary:
    //     Equivalent to HTTP status 300. System.Net.HttpStatusCode.MultipleChoices indicates
    //     that the requested information has multiple representations. The default action
    //     is to treat this status as a redirect and follow the contents of the Location
    //     header associated with this response. MultipleChoices is a synonym for Ambiguous.
    MultipleChoices = 300,

    // Summary:
    //     Equivalent to HTTP status 301. System.Net.HttpStatusCode.Moved indicates that
    //     the requested information has been moved to the URI specified in the Location
    //     header. The default action when this status is received is to follow the Location
    //     header associated with the response. When the original request method was POST,
    //     the redirected request will use the GET method. Moved is a synonym for MovedPermanently.
    Moved = 301,

    // Summary:
    //     Equivalent to HTTP status 301. System.Net.HttpStatusCode.MovedPermanently indicates
    //     that the requested information has been moved to the URI specified in the Location
    //     header. The default action when this status is received is to follow the Location
    //     header associated with the response. MovedPermanently is a synonym for Moved.
    MovedPermanently = 301,

    // Summary:
    //     Equivalent to HTTP status 302. System.Net.HttpStatusCode.Found indicates that
    //     the requested information is located at the URI specified in the Location header.
    //     The default action when this status is received is to follow the Location header
    //     associated with the response. When the original request method was POST, the
    //     redirected request will use the GET method. Found is a synonym for Redirect.
    Found = 302,

    // Summary:
    //     Equivalent to HTTP status 302. System.Net.HttpStatusCode.Redirect indicates that
    //     the requested information is located at the URI specified in the Location header.
    //     The default action when this status is received is to follow the Location header
    //     associated with the response. When the original request method was POST, the
    //     redirected request will use the GET method. Redirect is a synonym for Found.
    Redirect = 302,

    // Summary:
    //     Equivalent to HTTP status 303. System.Net.HttpStatusCode.RedirectMethod automatically
    //     redirects the client to the URI specified in the Location header as the result
    //     of a POST. The request to the resource specified by the Location header will
    //     be made with a GET. RedirectMethod is a synonym for SeeOther.
    RedirectMethod = 303,

    // Summary:
    //     Equivalent to HTTP status 303. System.Net.HttpStatusCode.SeeOther automatically
    //     redirects the client to the URI specified in the Location header as the result
    //     of a POST. The request to the resource specified by the Location header will
    //     be made with a GET. SeeOther is a synonym for RedirectMethod.
    SeeOther = 303,

    // Summary:
    //     Equivalent to HTTP status 304. System.Net.HttpStatusCode.NotModified indicates
    //     that the client's cached copy is up to date. The contents of the resource are
    //     not transferred.
    NotModified = 304,

    // Summary:
    //     Equivalent to HTTP status 305. System.Net.HttpStatusCode.UseProxy indicates that
    //     the request should use the proxy server at the URI specified in the Location
    //     header.
    UseProxy = 305,

    // Summary:
    //     Equivalent to HTTP status 306. System.Net.HttpStatusCode.Unused is a proposed
    //     extension to the HTTP/1.1 specification that is not fully specified.
    Unused = 306,

    // Summary:
    //     Equivalent to HTTP status 307. System.Net.HttpStatusCode.RedirectKeepVerb indicates
    //     that the request information is located at the URI specified in the Location
    //     header. The default action when this status is received is to follow the Location
    //     header associated with the response. When the original request method was POST,
    //     the redirected request will also use the POST method. RedirectKeepVerb is a synonym
    //     for TemporaryRedirect.
    RedirectKeepVerb = 307,

    // Summary:
    //     Equivalent to HTTP status 307. System.Net.HttpStatusCode.TemporaryRedirect indicates
    //     that the request information is located at the URI specified in the Location
    //     header. The default action when this status is received is to follow the Location
    //     header associated with the response. When the original request method was POST,
    //     the redirected request will also use the POST method. TemporaryRedirect is a
    //     synonym for RedirectKeepVerb.
    TemporaryRedirect = 307,

    // Summary:
    //     Equivalent to HTTP status 308. System.Net.HttpStatusCode.PermanentRedirect indicates
    //     that the request information is located at the URI specified in the Location
    //     header. The default action when this status is received is to follow the Location
    //     header associated with the response. When the original request method was POST,
    //     the redirected request will also use the POST method.
    PermanentRedirect = 308,

    // Summary:
    //     Equivalent to HTTP status 400. System.Net.HttpStatusCode.BadRequest indicates
    //     that the request could not be understood by the server. System.Net.HttpStatusCode.BadRequest
    //     is sent when no other error is applicable, or if the exact error is unknown or
    //     does not have its own error code.
    BadRequest = 400,

    // Summary:
    //     Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates
    //     that the requested resource requires authentication. The WWW-Authenticate header
    //     contains the details of how to perform the authentication.
    Unauthorized = 401,

    // Summary:
    //     Equivalent to HTTP status 402. System.Net.HttpStatusCode.PaymentRequired is reserved
    //     for future use.
    PaymentRequired = 402,

    // Summary:
    //     Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates
    //     that the server refuses to fulfill the request.
    Forbidden = 403,

    // Summary:
    //     Equivalent to HTTP status 404. System.Net.HttpStatusCode.NotFound indicates that
    //     the requested resource does not exist on the server.
    NotFound = 404,

    // Summary:
    //     Equivalent to HTTP status 405. System.Net.HttpStatusCode.MethodNotAllowed indicates
    //     that the request method (POST or GET) is not allowed on the requested resource.
    MethodNotAllowed = 405,

    // Summary:
    //     Equivalent to HTTP status 406. System.Net.HttpStatusCode.NotAcceptable indicates
    //     that the client has indicated with Accept headers that it will not accept any
    //     of the available representations of the resource.
    NotAcceptable = 406,

    // Summary:
    //     Equivalent to HTTP status 407. System.Net.HttpStatusCode.ProxyAuthenticationRequired
    //     indicates that the requested proxy requires authentication. The Proxy-authenticate
    //     header contains the details of how to perform the authentication.
    ProxyAuthenticationRequired = 407,

    // Summary:
    //     Equivalent to HTTP status 408. System.Net.HttpStatusCode.RequestTimeout indicates
    //     that the client did not send a request within the time the server was expecting
    //     the request.
    RequestTimeout = 408,

    // Summary:
    //     Equivalent to HTTP status 409. System.Net.HttpStatusCode.Conflict indicates that
    //     the request could not be carried out because of a conflict on the server.
    Conflict = 409,

    // Summary:
    //     Equivalent to HTTP status 410. System.Net.HttpStatusCode.Gone indicates that
    //     the requested resource is no longer available.
    Gone = 410,

    // Summary:
    //     Equivalent to HTTP status 411. System.Net.HttpStatusCode.LengthRequired indicates
    //     that the required Content-length header is missing.
    LengthRequired = 411,

    // Summary:
    //     Equivalent to HTTP status 412. System.Net.HttpStatusCode.PreconditionFailed indicates
    //     that a condition set for this request failed, and the request cannot be carried
    //     out. Conditions are set with conditional request headers like If-Match, If-None-Match,
    //     or If-Unmodified-Since.
    PreconditionFailed = 412,

    // Summary:
    //     Equivalent to HTTP status 413. System.Net.HttpStatusCode.RequestEntityTooLarge
    //     indicates that the request is too large for the server to process.
    RequestEntityTooLarge = 413,

    // Summary:
    //     Equivalent to HTTP status 414. System.Net.HttpStatusCode.RequestUriTooLong indicates
    //     that the URI is too long.
    RequestUriTooLong = 414,

    // Summary:
    //     Equivalent to HTTP status 415. System.Net.HttpStatusCode.UnsupportedMediaType
    //     indicates that the request is an unsupported type.
    UnsupportedMediaType = 415,

    // Summary:
    //     Equivalent to HTTP status 416. System.Net.HttpStatusCode.RequestedRangeNotSatisfiable
    //     indicates that the range of data requested from the resource cannot be returned,
    //     either because the beginning of the range is before the beginning of the resource,
    //     or the end of the range is after the end of the resource.
    RequestedRangeNotSatisfiable = 416,

    // Summary:
    //     Equivalent to HTTP status 417. System.Net.HttpStatusCode.ExpectationFailed indicates
    //     that an expectation given in an Expect header could not be met by the server.
    ExpectationFailed = 417,

    // Summary:
    //     Equivalent to HTTP status 421. System.Net.HttpStatusCode.MisdirectedRequest indicates
    //     that the request was directed at a server that is not able to produce a response.
    MisdirectedRequest = 421,

    // Summary:
    //     Equivalent to HTTP status 422. System.Net.HttpStatusCode.UnprocessableEntity
    //     indicates that the request was well-formed but was unable to be followed due
    //     to semantic errors.
    UnprocessableEntity = 422,

    // Summary:
    //     Equivalent to HTTP status 423. System.Net.HttpStatusCode.Locked indicates that
    //     the source or destination resource is locked.
    Locked = 423,

    // Summary:
    //     Equivalent to HTTP status 424. System.Net.HttpStatusCode.FailedDependency indicates
    //     that the method couldn't be performed on the resource because the requested action
    //     depended on another action and that action failed.
    FailedDependency = 424,

    // Summary:
    //     Equivalent to HTTP status 426. System.Net.HttpStatusCode.UpgradeRequired indicates
    //     that the client should switch to a different protocol such as TLS/1.0.
    UpgradeRequired = 426,

    // Summary:
    //     Equivalent to HTTP status 428. System.Net.HttpStatusCode.PreconditionRequired
    //     indicates that the server requires the request to be conditional.
    PreconditionRequired = 428,

    // Summary:
    //     Equivalent to HTTP status 429. System.Net.HttpStatusCode.TooManyRequests indicates
    //     that the user has sent too many requests in a given amount of time.
    TooManyRequests = 429,

    // Summary:
    //     Equivalent to HTTP status 431. System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge
    //     indicates that the server is unwilling to process the request because its header
    //     fields (either an individual header field or all the header fields collectively)
    //     are too large.
    RequestHeaderFieldsTooLarge = 431,

    // Summary:
    //     Equivalent to HTTP status 451. System.Net.HttpStatusCode.UnavailableForLegalReasons
    //     indicates that the server is denying access to the resource as a consequence
    //     of a legal demand.
    UnavailableForLegalReasons = 451,

    // Summary:
    //     Equivalent to HTTP status 500. System.Net.HttpStatusCode.InternalServerError
    //     indicates that a generic error has occurred on the server.
    InternalServerError = 500,

    // Summary:
    //     Equivalent to HTTP status 501. System.Net.HttpStatusCode.NotImplemented indicates
    //     that the server does not support the requested function.
    NotImplemented = 501,

    // Summary:
    //     Equivalent to HTTP status 502. System.Net.HttpStatusCode.BadGateway indicates
    //     that an intermediate proxy server received a bad response from another proxy
    //     or the origin server.
    BadGateway = 502,

    // Summary:
    //     Equivalent to HTTP status 503. System.Net.HttpStatusCode.ServiceUnavailable indicates
    //     that the server is temporarily unavailable, usually due to high load or maintenance.
    ServiceUnavailable = 503,

    // Summary:
    //     Equivalent to HTTP status 504. System.Net.HttpStatusCode.GatewayTimeout indicates
    //     that an intermediate proxy server timed out while waiting for a response from
    //     another proxy or the origin server.
    GatewayTimeout = 504,

    // Summary:
    //     Equivalent to HTTP status 505. System.Net.HttpStatusCode.HttpVersionNotSupported
    //     indicates that the requested HTTP version is not supported by the server.
    HttpVersionNotSupported = 505,

    // Summary:
    //     Equivalent to HTTP status 506. System.Net.HttpStatusCode.VariantAlsoNegotiates
    //     indicates that the chosen variant resource is configured to engage in transparent
    //     content negotiation itself and, therefore, isn't a proper endpoint in the negotiation
    //     process.
    VariantAlsoNegotiates = 506,

    // Summary:
    //     Equivalent to HTTP status 507. System.Net.HttpStatusCode.InsufficientStorage
    //     indicates that the server is unable to store the representation needed to complete
    //     the request.
    InsufficientStorage = 507,

    // Summary:
    //     Equivalent to HTTP status 508. System.Net.HttpStatusCode.LoopDetected indicates
    //     that the server terminated an operation because it encountered an infinite loop
    //     while processing a WebDAV request with "Depth: infinity". This status code is
    //     meant for backward compatibility with clients not aware of the 208 status code
    //     System.Net.HttpStatusCode.AlreadyReported appearing in multistatus response bodies.
    LoopDetected = 508,

    // Summary:
    //     Equivalent to HTTP status 510. System.Net.HttpStatusCode.NotExtended indicates
    //     that further extensions to the request are required for the server to fulfill
    //     it.
    NotExtended = 510,

    // Summary:
    //     Equivalent to HTTP status 511. System.Net.HttpStatusCode.NetworkAuthenticationRequired
    //     indicates that the client needs to authenticate to gain network access; it's
    //     intended for use by intercepting proxies used to control access to the network.
    NetworkAuthenticationRequired = 511
}

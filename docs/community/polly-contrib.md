# Polly-Contrib

Polly now has a [Polly-Contrib](https://github.com/Polly-Contrib) to allow the community to contribute policies or other enhancements around Polly with a low burden of ceremony.

Have a contrib you'd like to publish under Polly-Contrib? Contact us with  an issue here or on [Polly's Slack](http://pollytalk.slack.com), and we can set up a CI-ready Polly.Contrib repo to which you have full rights, to help you manage and deliver your awesomeness to the community!

We also provide:

- a blank [starter template for a custom policy](https://github.com/Polly-Contrib/Polly.Contrib.CustomPolicyTemplates) (see above for more on custom policies)
- a [template repo for any other contrib](https://github.com/Polly-Contrib/Polly.Contrib.BlankTemplate)

Both templates contain a full project structure referencing Polly, Polly's default build targets, and a build to build and test your contrib and make a NuGet package.

## Available via Polly-Contrib

- [Polly.Contrib.WaitAndRetry](https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry): a collection of concise helper methods for common wait-and-retry strategies; and a new jitter formula combining exponential backoff with a very even distribution of randomly-jittered retry intervals.
- [Polly.Contrib.AzureFunctions.CircuitBreaker](https://github.com/Polly-Contrib/Polly.Contrib.AzureFunctions.CircuitBreaker): a distributed circuit-breaker implemented in Azure Functions; consumable in Azure Functions, or from anywhere over http.
- [Simmy](https://github.com/Polly-Contrib/Simmy): our chaos engineering project.
- [Polly.Contrib.TimingPolicy](https://github.com/Polly-Contrib/Polly.Contrib.TimingPolicy): a starter policy to publish execution timings of any call executed through Policy.
- [Polly.Contrib.LoggingPolicy](https://github.com/Polly-Contrib/Polly.Contrib.LoggingPolicy): a policy simply to log handled exceptions/faults, and rethrow or bubble the fault outwards.

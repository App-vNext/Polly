# Chaos engineering with Simmy

[Simmy][simmy] is a major new companion project adding a chaos-engineering and fault-injection dimension to Polly, through the provision of policies to selectively inject faults, latency, custom behavior or fake results.

<img src="../../logos/Simmy-Logo.png" alt="Simmy"/>

# Motivation

There are a lot of questions when it comes to chaos-engineering and making sure that a system is actually ready to face the worst possible scenarios:

* Is my system resilient enough?
* Am I handling the right exceptions/scenarios?
* How will my system behave if X happens?
* How can I test without waiting for a handled (or even unhandled) exception to happen in my production environment?

Using Polly helps me introduce resilience to my project, but I don't want to have to wait for expected or unexpected failures to test it out. My resilience could be wrongly implemented; testing the scenarios is not straight forward; and mocking failure of some dependencies (for example a cloud SaaS or PaaS service) is not always straightforward.

**What do I need, to simulate chaotic scenarios in my production environment?**

* A way to mock failures of dependencies (any service dependency for example).
* Define when to fail based on some external factors - maybe global configuration or some rule.
* A way to revert easily, to control the blast radius.
* Production grade, to run this in a production or near-production system with automation.

# Chaos strategies (a.k.a Monkey strategies)
Chaos strategies (or Monkey strategies as we call them) are in essence a [Resilience strategy](../strategies/index.md#built-in-strategies), which means, as well as a *Resilience Strategy* is the minimum unit of resilience for Polly, a *Monkey Strategy* is the minimum unit of chaos for Simmy.

## Built-in strategies
|Strategy| Reactive| What does the policy do?|
| ------------- |------------- |------------- |
|**[Fault](fault.md)**|No|Injects exceptions in your system.|
|**[Result](#Inject-result)**|Yes|Substitute results to fake outcomes in your system.|
|**[Latency](latency.md)**|No|Injects latency into executions before the calls are made.|
|**[Behavior](behavior.md)**|No|Allows you to inject _any_ extra behaviour, before a call is placed. |

## Usage
It is usual to place the Monkey Strategy innermost in a Resilience Pipeline. By placing the monkey strategies innermost, they subvert the usual outbound call at the last minute, substituting their fault or adding extra latency, etc. The existing resilience strategies - further out in the `ResiliencePipeline` - still apply, so you can test how the Polly resilience strategies you have configured handle the chaos/faults injected by Simmy.

## Common options across strategies
All the strategies' options implement the [`MonkeyStrategyOptions`](xref:Polly.Simmy.MonkeyStrategyOptions) class as it contains the basic configuration for every monkey strategy.

| Property                  | Default Value | Description                                  |
| ------------------------- | ------------- | -------------------------------------------- |
| `InjectionRate`           | 0.001 ms      | A decimal between 0 and 1 inclusive. The strategy will inject the chaos, randomly, that proportion of the time, eg: if 0.2, twenty percent of calls will be randomly affected; if 0.01, one percent of calls; if 1, all calls.    |
| `InjectionRateGenerator`  | `null`        | Generates the injection rate for a given execution, which the value should be between [0, 1] (inclusive). |
| `Enabled`                 | `false`       | Determines whether the strategy is enabled or not.    |
| `EnabledGenerator`        | `null`        | the enable generator that indicates whether or not the chaos strategy is enabled for a given execution.     |

[simmy]: https://github.com/Polly-Contrib/Simmy


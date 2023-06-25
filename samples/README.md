# Polly Samples

This repository contains a solution with basic examples demonstrating the creation and utilization of Polly strategies.

- [`Intro`](./Intro) - This section serves as an introduction to Polly. It demonstrates how to use `ResilienceStrategyBuilder` to create a `ResilienceStrategy`, which can be used to execute various user-provided callbacks.
- [`GenericStrategies`](./GenericStrategies) - This example showcases how to use `ResilienceStrategyBuilder<T>` to create a generic `ResilienceStrategy<T>`.
- [`Retries`](./Retries) - This part explains how to configure a retry resilience strategy.
- [`Extensibility`](./Extensibility) - In this part, you can learn how Polly can be extended with custom resilience strategies.
- [`DependencyInjection`](./DependencyInjection) - This section demonstrates the integration of Polly with `IServiceCollection`.

These examples are designed as a quick-start guide to Polly. If you wish to explore more advanced scenarios and further enhance your learning, consider visiting the [Polly-Samples](https://github.com/App-vNext/Polly-Samples) repository.

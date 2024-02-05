# Polly Samples

This repository contains a solution with basic examples demonstrating the creation and utilization of Polly strategies.

- [`Intro`](./Intro) - This section serves as an introduction to Polly. It demonstrates how to use `ResiliencePipelineBuilder` to create a `ResiliencePipeline`, which can be used to execute various user-provided callbacks.
- [`GenericPipelines`](./GenericPipelines) - This example showcases how to use `ResiliencePipelineBuilder<T>` to create a generic `ResiliencePipeline<T>`.
- [`Retries`](./Retries) - This part explains how to configure a retry resilience strategy.
- [`Extensibility`](./Extensibility) - In this part, you can learn how Polly can be extended with custom resilience strategies.
- [`DependencyInjection`](./DependencyInjection) - This section demonstrates the integration of Polly with `IServiceCollection`.
- [`Chaos`](./Chaos) - Simple web application that communicates with an external service using HTTP client. It uses chaos strategies to inject chaos into HTTP client calls.

These examples are designed as a quick-start guide to Polly. If you wish to explore more advanced scenarios and further enhance your learning, consider visiting the [Polly-Samples](https://github.com/App-vNext/Polly-Samples) repository.

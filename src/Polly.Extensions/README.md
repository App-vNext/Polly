# Polly.Extensions overview

This project provides the following features:

- Incorporates [dependency injection](../../docs/dependency-injection.md) support and integrates with `IServiceCollection`.
- Offers [telemetry](../../docs/telemetry.md) support. This is achieved by implementing the `TelemetryListener` and utilizing it to translate the native Polly events into logs and metrics.

# Polly.Extensions overview

This project provides the following features:

- Incorporates [dependency injection](../../docs/advanced/dependency-injection.md) support and integrates with `IServiceCollection`.
- Offers [telemetry](../../docs/advanced/telemetry.md) support. This is achieved by implementing the `TelemetryListener` and utilizing it to translate the native Polly events into logs and metrics.

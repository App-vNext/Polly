# Extensibility

This article explains how to extend Polly with new [resilience strategies](../strategies/index.md). Polly recognizes two families of resilience strategies:

- **Reactive**: These strategies handle specific exceptions that are thrown, or results that are returned, by the callbacks executed through the strategy.
- **Proactive**: Unlike reactive strategies, proactive strategies do not focus on handling errors by the callbacks might throw or return. They can make proactive decisions to cancel or reject the execution of callbacks (e.g., using a rate limiter or a timeout resilience strategy).

This article will guide you through the process of creating a new demonstrative resilience strategy for each family type.

## Basics of extensibility

Irregardless of whether the strategy is reactive or proactive, every new resilience strategy should expose the following components:

- The options that describe the configuration of the strategy. These should derive from `ResilienceStrategyOptions`.
- Extensions for `ResiliencePipelineBuilder` or for `ResiliencePipelineBuilder<T>`.
- Custom arguments types used by delegates that hold the information about particular event.

## How to implement resilience strategy

Explore the documents bellow to learn more about strategy implementation details:

- [Proactive strategy](proactive-strategy.md): Describes the process of implementing proactive resilience strategy.
- [Reactive strategy](reactive-strategy.md): Describes the process of implementing reactive resilience strategy.

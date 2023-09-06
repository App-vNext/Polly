# Resilience Pipeline Registry

> [!NOTE]
> This is documentation for the upcoming Polly v8 release.

The `ResiliencePipelineRegistry<TKey>` is a generic class that provides the following functionalities:

- Thread-safe retrieval and dynamic creation of both generic and non-generic resilience pipelines.
- Dynamic reloading of resilience pipelines when configurations change.
- Support for registering both generic and non-generic resilience pipeline builders, enabling dynamic pipeline instance creation.
- Automatic resource management, including disposal of resources tied to resilience pipelines.

> [!NOTE]
> The generic `TKey` parameter specifies the key type used for caching individual resilience pipelines within the registry. In most use-cases, you will be working with `ResiliencePipelineRegistry<string>`.

## Usage

🚧 This documentation is being written as part of the Polly v8 release.

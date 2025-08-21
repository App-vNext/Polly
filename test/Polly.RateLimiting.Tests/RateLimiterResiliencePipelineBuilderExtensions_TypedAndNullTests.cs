using System;
using System.ComponentModel.DataAnnotations;
using Polly.Testing;
using Shouldly;

namespace Polly.RateLimiting.Tests
{
    public class RateLimiterResiliencePipelineBuilderExtensions_TypedAndNullTests
    {
        [Fact]
        public void TypedBuilder_AddConcurrencyLimiter_BuildsRateLimiterStrategy()
        {
            var builder = new ResiliencePipelineBuilder<int>()
                .AddConcurrencyLimiter(permitLimit: 2, queueLimit: 1);

            builder.Build()
                .GetPipelineDescriptor()
                .FirstStrategy
                .StrategyInstance
                .ShouldBeOfType<RateLimiterResilienceStrategy>();
        }

        [Fact]
        public void AddRateLimiter_WithNullLimiter_Throws()
        {
            var builder = new ResiliencePipelineBuilder();
            Should.Throw<ArgumentNullException>(() => builder.AddRateLimiter(limiter: null!));
        }

        [Fact]
        public void AddRateLimiter_WithNullOptions_Throws()
        {
            var builder = new ResiliencePipelineBuilder();
            Should.Throw<ArgumentNullException>(() => builder.AddRateLimiter(options: null!));
        }

        [Fact]
        public void AddConcurrencyLimiter_WithNullOptions_Throws()
        {
            var builder = new ResiliencePipelineBuilder();
            Should.Throw<ArgumentNullException>(() => builder.AddConcurrencyLimiter(options: null!));
        }

        [Fact]
        public void AddRateLimiter_InvalidOptions_ThrowsValidation()
        {
            var builder = new ResiliencePipelineBuilder();
            var options = new RateLimiterStrategyOptions { DefaultRateLimiterOptions = null! };

            Should.Throw<ValidationException>(() => builder.AddRateLimiter(options));
        }
    }
}

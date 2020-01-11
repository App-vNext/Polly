using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Polly.Specs.Executables
{
    public class ExecutablesAsyncSpecs
    {
        [Fact]
        public async Task Should_execute_user_delegate_with_input_parameter()
        {
            var policy = Policy.NoOpAsync();

            Int16 input1 = 1;
            Int64 captured = 0;

            await policy.ExecuteAsync<Int16>((context, token, captureContext, input) =>
                {
                    captured = input;
                    return Task.CompletedTask;
                }, new Context(),
                CancellationToken.None,
                continueOnCapturedContext: false,
                input1);

            captured.Should().Be(input1);
        }

        [Fact]
        public async Task Should_execute_user_delegate_with_two_input_parameters()
        {
            var policy = Policy.NoOpAsync();

            Int16 input1 = 1;
            Int32 input2 = 2;
            Int64 captured = 0;

            await policy.ExecuteAsync<Int16, Int32>((context, token, captureContext, t1, t2) =>
                {
                    captured = t1 + t2;
                    return Task.CompletedTask;
                }, new Context(),
                CancellationToken.None,
                continueOnCapturedContext: false,
                input1,
                input2);

            captured.Should().Be(input1 + input2);
        }

        [Fact]
        public async Task Should_execute_user_delegate_with_input_parameter_and_return_type()
        {
            var policy = Policy.NoOpAsync();

            Int16 input1 = 1;
            Int64 captured = await policy.ExecuteAsync<Int16, Int64>(async (context, token, captureContext, input) =>
                {
                    await Task.CompletedTask;
                    return input;
                }, new Context(),
                CancellationToken.None,
                continueOnCapturedContext: false,
                input1);

            captured.Should().Be(input1);
        }

        [Fact]
        public async Task Should_execute_user_delegate_with_two_input_parameters_and_return_type()
        {
            var policy = Policy.NoOpAsync();

            Int16 input1 = 1;
            Int32 input2 = 2;
            Int64 captured = await policy.ExecuteAsync<Int16, Int32, Int64>(async (context, token, captureContext, t1, t2) =>
                {
                    await Task.CompletedTask;
                    return t1 + t2;
                }, new Context(),
                CancellationToken.None,
                continueOnCapturedContext: false,
                input1,
                input2);

            captured.Should().Be(input1 + input2);
        }

        [Fact]
        public async Task Should_executeandcapture_user_delegate_with_input_parameter()
        {
            var policy = Policy.NoOpAsync();

            Int16 input1 = 1;
            Int64 captured = 0;

            var policyResult = await policy.ExecuteAndCaptureAsync<Int16>((context, token, captureContext, input) =>
                {
                    captured = input;
                    return Task.CompletedTask;
                }, new Context(),
                CancellationToken.None,
                continueOnCapturedContext: false,
                input1);

            policyResult.Outcome.Should().Be(OutcomeType.Successful);
            captured.Should().Be(input1);
        }

        [Fact]
        public async Task Should_executeandcapture_user_delegate_with_two_input_parameters()
        {
            var policy = Policy.NoOpAsync();

            Int16 input1 = 1;
            Int32 input2 = 2;
            Int64 captured = 0;

            var policyResult = await policy.ExecuteAndCaptureAsync<Int16, Int32>((context, token, captureContext, t1, t2) =>
                {
                    captured = t1 + t2;
                    return Task.CompletedTask;
                }, new Context(),
                CancellationToken.None,
                continueOnCapturedContext: false,
                input1,
                input2);

            policyResult.Outcome.Should().Be(OutcomeType.Successful);
            captured.Should().Be(input1 + input2);
        }

        [Fact]
        public async Task Should_executeandcapture_user_delegate_with_input_parameter_and_return_type()
        {
            var policy = Policy.NoOpAsync();

            Int16 input1 = 1;
            var policyResult = await policy.ExecuteAndCaptureAsync<Int16, Int64>(async (context, token, captureContext, input) =>
                {
                    await Task.CompletedTask;
                    return input;
                }, new Context(),
                CancellationToken.None,
                continueOnCapturedContext: false,
                input1);

            policyResult.Outcome.Should().Be(OutcomeType.Successful);
            policyResult.Result.Should().Be(input1);
        }

        [Fact]
        public async Task Should_executeandcapture_user_delegate_with_two_input_parameters_and_return_type()
        {
            var policy = Policy.NoOpAsync();

            Int16 input1 = 1;
            Int32 input2 = 2;
            var policyResult = await policy.ExecuteAndCaptureAsync<Int16, Int32, Int64>(async (context, token, captureContext, t1, t2) =>
                {
                    await Task.CompletedTask;
                    return t1 + t2;
                }, new Context(),
                CancellationToken.None,
                continueOnCapturedContext: false,
                input1,
                input2);

            policyResult.Outcome.Should().Be(OutcomeType.Successful);
            policyResult.Result.Should().Be(input1 + input2);
        }
    }
}

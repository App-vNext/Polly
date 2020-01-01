using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.NoOp;
using Xunit;

namespace Polly.Specs.Executables
{
    public class ExecutablesTResultAsyncSpecs
    {

        [Fact]
        public async Task Should_execute_user_delegate_with_input_parameter_and_return_type()
        {
            AsyncNoOpPolicy<Int64> policy = Policy.NoOpAsync<Int64>() as AsyncNoOpPolicy<Int64>;

            Int16 input1 = 1;
            Int64 captured = await policy.ExecuteAsync<Int16>(async (context, token, captureContext, input) =>
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
            AsyncNoOpPolicy<Int64> policy = Policy.NoOpAsync<Int64>() as AsyncNoOpPolicy<Int64>;

            Int16 input1 = 1;
            Int32 input2 = 2;
            Int64 captured = await policy.ExecuteAsync<Int16, Int32>(async (context, token, captureContext, t1, t2) =>
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
        public async Task Should_executeandcapture_user_delegate_with_input_parameter_and_return_type()
        {
            AsyncNoOpPolicy<Int64> policy = Policy.NoOpAsync<Int64>() as AsyncNoOpPolicy<Int64>;

            Int16 input1 = 1;
            var policyResult = await policy.ExecuteAndCaptureAsync<Int16>(async (context, token, captureContext, input) =>
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
            AsyncNoOpPolicy<Int64> policy = Policy.NoOpAsync<Int64>() as AsyncNoOpPolicy<Int64>;

            Int16 input1 = 1;
            Int32 input2 = 2;
            var policyResult = await policy.ExecuteAndCaptureAsync<Int16, Int32>(async (context, token, captureContext, t1, t2) =>
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

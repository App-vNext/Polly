using System;
using System.Threading;
using FluentAssertions;
using Polly.NoOp;
using Xunit;

namespace Polly.Specs.Executables
{
    public class ExecutablesSyncSpecs
    {
        [Fact]
        public void Should_execute_user_delegate_with_input_parameter()
        {
            NoOpPolicy policy = Policy.NoOp() as NoOpPolicy;

            Int16 input1 = 1;
            Int64 captured = 0;

            policy.Execute<Int16>((context, token, input) =>
                {
                    captured = input;
                }, new Context(),
                CancellationToken.None,
                input1);

            captured.Should().Be(input1);
        }

        [Fact]
        public void Should_execute_user_delegate_with_two_input_parameters()
        {
            NoOpPolicy policy = Policy.NoOp() as NoOpPolicy;

            Int16 input1 = 1;
            Int32 input2 = 2;
            Int64 captured = 0;

            policy.Execute<Int16, Int32>((context, token, t1, t2) =>
                {
                    captured = t1 + t2;
                }, new Context(),
                CancellationToken.None,
                input1,
                input2);

            captured.Should().Be(input1 + input2);
        }

        [Fact]
        public void Should_execute_user_delegate_with_input_parameter_and_return_type()
        {
            NoOpPolicy policy = Policy.NoOp() as NoOpPolicy;

            Int16 input1 = 1;
            double captured = policy.Execute<Int16, Int64>((context, token, input) => input, new Context(),
                CancellationToken.None,
                input1);

            captured.Should().Be(input1);
        }

        [Fact]
        public void Should_execute_user_delegate_with_two_input_parameters_and_return_type()
        {
            NoOpPolicy policy = Policy.NoOp() as NoOpPolicy;

            Int16 input1 = 1;
            Int32 input2 = 2;
            double captured = policy.Execute<Int16, Int32, Int64>((context, token, t1, t2) => t1 + t2, new Context(),
                CancellationToken.None,
                input1,
                input2);

            captured.Should().Be(input1 + input2);
        }

        [Fact]
        public void Should_executeandcapture_user_delegate_with_input_parameter()
        {
            NoOpPolicy policy = Policy.NoOp() as NoOpPolicy;

            Int16 input1 = 1;
            double captured = 0;

            var policyResult = policy.ExecuteAndCapture<Int16>((context, token, input) =>
                {
                    captured = input;
                }, new Context(),
                CancellationToken.None,
                input1);

            policyResult.Outcome.Should().Be(OutcomeType.Successful);
            captured.Should().Be(input1);
        }

        [Fact]
        public void Should_executeandcapture_user_delegate_with_two_input_parameters()
        {
            NoOpPolicy policy = Policy.NoOp() as NoOpPolicy;

            Int16 input1 = 1;
            Int32 input2 = 2;
            double captured = 0;

            var policyResult = policy.ExecuteAndCapture<Int16, Int32>((context, token, t1, t2) =>
                {
                    captured = t1 + t2;
                }, new Context(),
                CancellationToken.None,
                input1,
                input2);

            policyResult.Outcome.Should().Be(OutcomeType.Successful);
            captured.Should().Be(input1 + input2);
        }

        [Fact]
        public void Should_executeandcapture_user_delegate_with_input_parameter_and_return_type()
        {
            NoOpPolicy policy = Policy.NoOp() as NoOpPolicy;

            Int16 input1 = 1;
            var policyResult = policy.ExecuteAndCapture<Int16, Int64>((context, token, input) => input, new Context(),
                CancellationToken.None,
                input1);

            policyResult.Outcome.Should().Be(OutcomeType.Successful);
            policyResult.Result.Should().Be(input1);
        }

        [Fact]
        public void Should_executeandcapture_user_delegate_with_two_input_parameters_and_return_type()
        {
            NoOpPolicy policy = Policy.NoOp() as NoOpPolicy;

            Int16 input1 = 1;
            Int32 input2 = 2;
            var policyResult = policy.ExecuteAndCapture<Int16, Int32, Int64>((context, token, t1, t2) => t1 + t2, new Context(),
                CancellationToken.None,
                input1,
                input2);

            policyResult.Outcome.Should().Be(OutcomeType.Successful);
            policyResult.Result.Should().Be(input1 + input2);
        }
    }
}

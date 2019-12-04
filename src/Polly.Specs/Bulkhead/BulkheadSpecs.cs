﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Polly.Bulkhead;
using Polly.Specs.Helpers.Bulkhead;

using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Polly.Specs.Bulkhead
{
    [Collection(Polly.Specs.Helpers.Constants.ParallelThreadDependentTestCollection)]
    public class BulkheadSpecs : BulkheadSpecsBase
    {
        public BulkheadSpecs(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        #region Configuration

        [Fact]
        public void Should_throw_when_maxparallelization_less_or_equal_to_zero()
        {
            Action policy = () => Policy
                .Bulkhead(0, 1);

            policy.Should().Throw<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("maxParallelization");
        }

        [Fact]
        public void Should_throw_when_maxqueuedactions_less_than_zero()
        {
            Action policy = () => Policy
                .Bulkhead(1, -1);

            policy.Should().Throw<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("maxQueuingActions");
        }

        [Fact]
        public void Should_throw_when_onBulkheadRejected_is_null()
        {
            Action policy = () => Policy
                .Bulkhead(1, 0, null);

            policy.Should().Throw<ArgumentNullException>().And
                .ParamName.Should().Be("onBulkheadRejected");
        }

        #endregion

        #region onBulkheadRejected delegate

        [Fact]
        public void Should_call_onBulkheadRejected_with_passed_context()
        {
            string operationKey = "SomeKey";
            Context contextPassedToExecute = new Context(operationKey);

            Context contextPassedToOnRejected = null;
            Action<Context> onRejected = ctx => { contextPassedToOnRejected = ctx; };

            using (BulkheadPolicy bulkhead = Policy.Bulkhead(1, onRejected))
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

                Task.Run(() => { bulkhead.Execute(() => { tcs.Task.Wait(); }); });

                // Time for the other thread to kick up and take the bulkhead.
                Within(CohesionTimeLimit, () => Expect(0, () => bulkhead.BulkheadAvailableCount, nameof(bulkhead.BulkheadAvailableCount)));

                bulkhead.Invoking(b => b.Execute(ctx => { }, contextPassedToExecute)).Should()
                    .Throw<BulkheadRejectedException>();

                tcs.SetCanceled();

                contextPassedToOnRejected.Should().NotBeNull();
                contextPassedToOnRejected.OperationKey.Should().Be(operationKey);
                contextPassedToOnRejected.Should().BeSameAs(contextPassedToExecute);
            }
        }

        #endregion

        #region Bulkhead behaviour

        protected override IBulkheadPolicy GetBulkhead(int maxParallelization, int maxQueuingActions)
        {
            return Policy.Bulkhead(maxParallelization, maxQueuingActions);
        }

        protected override Task ExecuteOnBulkhead(IBulkheadPolicy bulkhead, TraceableAction action)
        {
            return action.ExecuteOnBulkhead((BulkheadPolicy) bulkhead);
        }

        #endregion

    }
}

﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly.Bulkhead;
using Polly.Specs.Helpers;
using Polly.Specs.Helpers.Bulkhead;

using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Polly.Specs.Bulkhead
{
    [Collection(Polly.Specs.Helpers.Constants.ParallelThreadDependentTestCollection)]
    public class BulkheadTResultSpecs : BulkheadSpecsBase
    {
        public BulkheadTResultSpecs(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        #region Configuration

        [Fact]
        public void Should_throw_when_maxparallelization_less_or_equal_to_zero()
        {
            Action policy = () => Policy
                .Bulkhead<int>(0, 1);

            policy.Should().Throw<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("maxParallelization");
        }

        [Fact]
        public void Should_throw_when_maxQueuingActions_less_than_zero()
        {
            Action policy = () => Policy
                .Bulkhead<int>(1, -1);

            policy.Should().Throw<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("maxQueuingActions");
        }

        [Fact]
        public void Should_throw_when_onBulkheadRejected_is_null()
        {
            Action policy = () => Policy
                .Bulkhead<int>(1, 0, null);

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

            using (BulkheadPolicy<int> bulkhead = Policy.Bulkhead<int>(1, onRejected))
            { 
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                using (CancellationTokenSource cancellationSource = new CancellationTokenSource())
                {
                    Task.Run(() => {
                        bulkhead.Execute(() =>
                        {
                            tcs.Task.Wait();
                            return 0;
                        });
                    });

                    Within(CohesionTimeLimit, () => Expect(0, () => bulkhead.BulkheadAvailableCount, nameof(bulkhead.BulkheadAvailableCount)));

                    bulkhead.Invoking(b => b.Execute(ctx => 1, contextPassedToExecute)).Should().Throw<BulkheadRejectedException>();

                    cancellationSource.Cancel();
                    tcs.SetCanceled();
                }

                contextPassedToOnRejected.Should().NotBeNull();
                contextPassedToOnRejected.OperationKey.Should().Be(operationKey);
                contextPassedToOnRejected.Should().BeSameAs(contextPassedToExecute);
            }
        }

        #endregion

        #region Bulkhead behaviour

        protected override IBulkheadPolicy GetBulkhead(int maxParallelization, int maxQueuingActions)
        {
            return Policy.Bulkhead<ResultPrimitive>(maxParallelization, maxQueuingActions);
        }

        protected override Task ExecuteOnBulkhead(IBulkheadPolicy bulkhead, TraceableAction action)
        {
            return action.ExecuteOnBulkhead<ResultPrimitive>((BulkheadPolicy<ResultPrimitive>) bulkhead);
        }

        #endregion
    }
}

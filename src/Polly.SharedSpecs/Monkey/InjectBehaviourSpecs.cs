using System;
using FluentAssertions;
using Polly.Monkey;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Monkey
{
    [Collection(Polly.Specs.Helpers.Constants.AmbientContextDependentTestCollection)]
    public class InjectBehaviourSpecs : IDisposable
    {
        public InjectBehaviourSpecs()
        {
            ThreadSafeRandom_LockOncePerThread.NextDouble = () => 0.5;
        }

        public void Dispose()
        {
            ThreadSafeRandom_LockOncePerThread.Reset();
        }

        #region Action based Monkey policies

        [Fact]
        public void Given_not_enabled_should_not_inject_behaviour()
        {
            Boolean userDelegateExecuted = false;
            Boolean injectedBehaviourExecuted = false;

            var policy = MonkeyPolicy.InjectBehaviour(() => { injectedBehaviourExecuted = true; }, 0.6, () => false);

            policy.Execute(() => { userDelegateExecuted = true; });

            userDelegateExecuted.Should().BeTrue();
            injectedBehaviourExecuted.Should().BeFalse();
        }

        [Fact]
        public void Given_enabled_and_randomly_within_threshold_should_inject_behaviour()
        {
            Boolean userDelegateExecuted = false;
            Boolean injectedBehaviourExecuted = false;

            var policy = MonkeyPolicy.InjectBehaviour(() => { injectedBehaviourExecuted = true; }, 0.6, () => true);

            policy.Execute(() => { userDelegateExecuted = true; });

            userDelegateExecuted.Should().BeTrue();
            injectedBehaviourExecuted.Should().BeTrue();
        }

        [Fact]
        public void Given_enabled_and_randomly_not_within_threshold_should_not_inject_behaviour()
        {
            Boolean userDelegateExecuted = false;
            Boolean injectedBehaviourExecuted = false;

            var policy = MonkeyPolicy.InjectBehaviour(() => { injectedBehaviourExecuted = true; }, 0.4, () => true);

            policy.Execute(() => { userDelegateExecuted = true; });

            userDelegateExecuted.Should().BeTrue();
            injectedBehaviourExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_inject_behaviour_before_executing_user_delegate()
        {
            Boolean userDelegateExecuted = false;
            Boolean injectedBehaviourExecuted = false;

            var policy = MonkeyPolicy.InjectBehaviour(() =>
            {
                userDelegateExecuted.Should().BeFalse(); // Not yet executed at the time the injected behaviour runs.
                injectedBehaviourExecuted = true;
            }, 0.6, () => true);

            policy.Execute(() => { userDelegateExecuted = true; });

            userDelegateExecuted.Should().BeTrue();
            injectedBehaviourExecuted.Should().BeTrue();
        }

        #endregion
    }
}

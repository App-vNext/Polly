using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Polly.Specs.Wrap
{
    public static partial class CustomPolicySyntax
    {
        public static CustomSyncPolicy Custom(this PolicyBuilder policyBuilder)
        {
            return new CustomSyncPolicy(policyBuilder);
        }

        public static CustomSyncPolicy<TResult> Custom<TResult>(this PolicyBuilder<TResult> policyBuilder)
        {
            return new CustomSyncPolicy<TResult>(policyBuilder);
        }
    }

    public class CustomSyncPolicy : ISyncPolicy
    {
        public CustomSyncPolicy(PolicyBuilder policyBuilder)
        {
            this.ExceptionPredicates = policyBuilder.ExceptionPredicates;
        }

        public IEnumerable<ExceptionPredicate> ExceptionPredicates { get; private set; }

        public string PolicyKey { get; private set; }

        public void Execute(Action action)
        {
            // Do nothing, succeed
        }

        public void Execute(Action<Context> action, IDictionary<string, object> contextData)
        {
            action(new Context(contextData));
        }

        public void Execute(Action<Context> action, Context context)
        {
            action(context);
        }

        public void Execute(Action<CancellationToken> action, CancellationToken cancellationToken)
        {
            action(cancellationToken);
        }

        public void Execute(Action<Context, CancellationToken> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            action(new Context(contextData), cancellationToken);
        }

        public void Execute(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken)
        {
            action(context, cancellationToken);
        }

        public TResult Execute<TResult>(Func<TResult> action)
        {
            return default(TResult);
        }

        public TResult Execute<TResult>(Func<Context, TResult> action, IDictionary<string, object> contextData)
        {
            return default(TResult);
        }

        public TResult Execute<TResult>(Func<Context, TResult> action, Context context)
        {
            return default(TResult);
        }

        public TResult Execute<TResult>(Func<CancellationToken, TResult> action, CancellationToken cancellationToken)
        {
            return default(TResult);
        }

        public TResult Execute<TResult>(Func<Context, CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return default(TResult);
        }

        public TResult Execute<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            return default(TResult);
        }

        public PolicyResult ExecuteAndCapture(Action action)
        {
            return PolicyResult.Successful(new Context());
        }

        public PolicyResult ExecuteAndCapture(Action<Context> action, IDictionary<string, object> contextData)
        {
            return PolicyResult.Successful(new Context(contextData));
        }

        public PolicyResult ExecuteAndCapture(Action<Context> action, Context context)
        {
            return PolicyResult.Successful(context);
        }

        public PolicyResult ExecuteAndCapture(Action<CancellationToken> action, CancellationToken cancellationToken)
        {
            return PolicyResult.Successful(new Context());
        }

        public PolicyResult ExecuteAndCapture(Action<Context, CancellationToken> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return PolicyResult.Successful(new Context(contextData));
        }

        public PolicyResult ExecuteAndCapture(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken)
        {
            return PolicyResult.Successful(context);
        }

        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<TResult> action)
        {
            return PolicyResult<TResult>.Successful(default(TResult), new Context());
        }

        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, TResult> action, IDictionary<string, object> contextData)
        {
            return PolicyResult<TResult>.Successful(default(TResult), new Context(contextData));
        }

        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, TResult> action, Context context)
        {
            return PolicyResult<TResult>.Successful(default(TResult), context);
        }

        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<CancellationToken, TResult> action, CancellationToken cancellationToken)
        {
            return PolicyResult<TResult>.Successful(default(TResult), new Context());
        }

        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return PolicyResult<TResult>.Successful(default(TResult), new Context(contextData));
        }

        public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            return PolicyResult<TResult>.Successful(default(TResult), context);
        }

        public ISyncPolicy WithPolicyKey(string policyKey)
        {
            this.PolicyKey = policyKey;
            return this;
        }
    };

    public class CustomSyncPolicy<TResult> : ISyncPolicy<TResult>
    {
        public CustomSyncPolicy(PolicyBuilder<TResult> policyBuilder)
        {
            this.ExceptionPredicates = policyBuilder.ExceptionPredicates;
            this.ResultPredicates = policyBuilder.ResultPredicates;
        }

        public IEnumerable<ExceptionPredicate> ExceptionPredicates { get; private set; }

        public IEnumerable<ResultPredicate<TResult>> ResultPredicates { get; private set; }

        public string PolicyKey { get; private set; }

        public TResult Execute(Func<TResult> action)
        {
            return action();
        }

        public TResult Execute(Func<Context, TResult> action, IDictionary<string, object> contextData)
        {
            return action(new Context(contextData));
        }

        public TResult Execute(Func<Context, TResult> action, Context context)
        {
            return action(context);
        }

        public TResult Execute(Func<CancellationToken, TResult> action, CancellationToken cancellationToken)
        {
            return action(cancellationToken);
        }

        public TResult Execute(Func<Context, CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return action(new Context(contextData), cancellationToken);
        }

        public TResult Execute(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            return action(context, cancellationToken);

        }

        public PolicyResult<TResult> ExecuteAndCapture(Func<TResult> action)
        {
            return ExecuteAndCapture((ctx, ct) => action(), new Context(), CancellationToken.None);
        }

        public PolicyResult<TResult> ExecuteAndCapture(Func<Context, TResult> action, IDictionary<string, object> contextData)
        {
            return ExecuteAndCapture((ctx, ct) => action(ctx), new Context(contextData), CancellationToken.None);
        }

        public PolicyResult<TResult> ExecuteAndCapture(Func<Context, TResult> action, Context context)
        {
            return ExecuteAndCapture((ctx, ct) => action(ctx), context, CancellationToken.None);
        }

        public PolicyResult<TResult> ExecuteAndCapture(Func<CancellationToken, TResult> action, CancellationToken cancellationToken)
        {
            return ExecuteAndCapture((ctx, ct) => action(cancellationToken), new Context(), CancellationToken.None);
        }

        public PolicyResult<TResult> ExecuteAndCapture(Func<Context, CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            return ExecuteAndCapture((ctx, ct) => action(ctx, cancellationToken), new Context(contextData), CancellationToken.None);
        }

        public PolicyResult<TResult> ExecuteAndCapture(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                return PolicyResult<TResult>.Successful(Execute(action, context, cancellationToken), context);
            }
            catch (Exception exception)
            {
                return PolicyResult<TResult>.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
            }
        }

        private static ExceptionType GetExceptionType(IEnumerable<ExceptionPredicate> exceptionPredicates, Exception exception)
        {
            var isExceptionTypeHandledByThisPolicy = exceptionPredicates.Any(predicate => predicate(exception));

            return isExceptionTypeHandledByThisPolicy
                ? ExceptionType.HandledByThisPolicy
                : ExceptionType.Unhandled;
        }

        public ISyncPolicy<TResult> WithPolicyKey(string policyKey)
        {
            this.PolicyKey = policyKey;
            return this;
        }
    }
}

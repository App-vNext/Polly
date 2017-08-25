using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Specs.Wrap
{
    public static partial class CustomPolicySyntax
    {
        public static CustomAsyncPolicy CustomAsync(this PolicyBuilder policyBuilder)
        {
            return new CustomAsyncPolicy(policyBuilder);
        }

        public static CustomAsyncPolicy<TResult> CustomAsync<TResult>(this PolicyBuilder<TResult> policyBuilder)
        {
            return new CustomAsyncPolicy<TResult>(policyBuilder);
        }
    }

    public class CustomAsyncPolicy : IAsyncPolicy
    {
        public CustomAsyncPolicy(PolicyBuilder policyBuilder)
        {
            this.ExceptionPredicates = policyBuilder.ExceptionPredicates;
        }

        public IEnumerable<ExceptionPredicate> ExceptionPredicates { get; private set; }

        public string PolicyKey { get; private set; }

        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Task> action)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, Task> action, IDictionary<string, object> contextData)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, Task> action, Context context)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> action)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, Task<TResult>> action, Context context)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(Func<Task> action)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(Func<Context, Task> action, IDictionary<string, object> contextData)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(Func<Context, Task> action, Context context)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<Context, Task<TResult>> action, Context context)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public IAsyncPolicy WithPolicyKey(string policyKey)
        {
            this.PolicyKey = policyKey;
            return this;
        }
    };

    public class CustomAsyncPolicy<TResult> : IAsyncPolicy<TResult>
    {
        
        public CustomAsyncPolicy(PolicyBuilder<TResult> policyBuilder)
        {
            this.ExceptionPredicates = policyBuilder.ExceptionPredicates;
            this.ResultPredicates = policyBuilder.ResultPredicates;
        }

        public IEnumerable<ExceptionPredicate> ExceptionPredicates { get; private set; }

        public IEnumerable<ResultPredicate<TResult>> ResultPredicates { get; private set; }

        public string PolicyKey { get; private set; }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Task<TResult>> action)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, Task<TResult>> action, Context context)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync(Func<Task<TResult>> action)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync(Func<Context, Task<TResult>> action, Context context)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            throw new NotImplementedException();
        }

        public IAsyncPolicy<TResult> WithPolicyKey(string policyKey)
        {
            this.PolicyKey = policyKey;
            return this;
        }
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace Polly
{
    /// <summary>
    /// Defines an operation that can be executed asynchronously with no return value.
    /// </summary>
    public interface IAsyncExecutable : IAsyncExecutable<object>
    {
    }

    /// <summary>
    /// Defines an operation that can be executed asynchronously to return a promise of a result of type <typeparamref name="TResult"/>
    /// </summary>
    /// <typeparam name="TResult">The return type of the operation.</typeparam>
    public interface IAsyncExecutable<TResult>
    {
        /// <summary>
        /// Asynchronously executes the operation represented by the instance.
        /// </summary>
        /// <param name="context">The Polly execution <see cref="Context"/> to execute the operation with.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> governing cancellation of the executed operation.</param>
        /// <param name="continueOnCapturedContext">Whether continuing after an awaited operation should continue of a captured <see cref="SynchronizationContext"/></param>
        /// <returns>A promise of a result of type <typeparamref name="TResult"/></returns>
        Task<TResult> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext);
    }
}
using System.Threading;

namespace Polly
{
    /// <summary>
    /// Defines an operation that can be executed synchronously to return a result of type <typeparamref name="TResult"/>
    /// </summary>
    /// <typeparam name="TResult">The return type of the operation.</typeparam>
    public interface ISyncExecutable<out TResult>
    {
        /// <summary>
        /// Synchronously executes the operation represented by the instance.
        /// </summary>
        /// <param name="context">The Polly execution <see cref="Context"/> to execute the operation with.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> governing cancellation of the executed operation.</param>
        /// <returns>A result of type <typeparamref name="TResult"/></returns>
        TResult Execute(Context context, CancellationToken cancellationToken);
    }

}

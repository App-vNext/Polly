namespace Polly.Utils;

#pragma warning disable CA1716 // Identifiers should not match keywords

/// <summary>
/// Represents a pool of objects.
/// </summary>
/// <typeparam name="T">The type of objects in the pool.</typeparam>
internal interface IObjectPool<T>
    where T : class
{
    /// <summary>
    /// Gets an object from the pool.
    /// </summary>
    /// <returns>Object instance.</returns>
    T Get();

    /// <summary>
    /// Returns an object to the pool.
    /// </summary>
    /// <param name="obj">The object instance to return.</param>
    void Return(T obj);
}

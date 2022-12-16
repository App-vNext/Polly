namespace Polly.Specs.Helpers
{
    /// <summary>
    /// A helper class supporting tests on how Policy&lt;TResult&gt; policies may handle return results which are primitive types such as ints or enums.
    /// </summary>
    internal enum ResultPrimitive
    {
        Undefined,
        Fault,
        Good,
        FaultAgain,
        GoodAgain,
        FaultYetAgain,
        Substitute,
        WhateverButTooLate
    }
}

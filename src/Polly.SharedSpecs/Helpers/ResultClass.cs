using System;
using System.Collections.Generic;
using System.Text;

namespace Polly.Specs.Helpers
{
    /// <summary>
    /// A helper class supporting tests on how Policy&lt;TResult&gt; policies handle return results which are class types (as opposed to primitive types).
    /// </summary>
    internal class ResultClass
    {
        public ResultClass(ResultPrimitive resultCode)
        {
            ResultCode = resultCode;
        }

        public ResultClass(ResultPrimitive resultCode, string someString) : this(resultCode)
        {
            SomeString = someString;
        }

        public ResultPrimitive ResultCode { get; set; }

        public string SomeString { get; set; }
    }
}

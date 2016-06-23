using System;
using System.Collections.Generic;
using System.Text;

namespace Polly.Specs.Helpers
{
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

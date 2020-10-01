using System;

namespace UnityStandardAssets.Utility.Exceptions
{
    // Derive an exception with a predefined message.
    class NotImplementedException : Exception
    {
        public NotImplementedException(string methodName) :
            base("Method " + methodName + " is not implemented yet."){ }
    }
}
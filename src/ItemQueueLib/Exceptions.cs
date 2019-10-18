using System;
using System.Runtime.Serialization;

namespace SMC.Utilities.Queues
{
    [Serializable]
    public class NoActionException : Exception
    {
        public NoActionException() { }

        public NoActionException(string message) : base(message) { }

        public NoActionException(string message, Exception innerException) : base(message, innerException) { }

        public NoActionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
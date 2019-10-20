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

    [Serializable]
    public class NoFunctionException : Exception
    {
        public NoFunctionException() { }

        public NoFunctionException(string message) : base(message) { }

        public NoFunctionException(string message, Exception innerException) : base(message, innerException) { }

        public NoFunctionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class NoPredicateException : Exception
    {
        public NoPredicateException() { }

        public NoPredicateException(string message) : base(message) { }

        public NoPredicateException(string message, Exception innerException) : base(message, innerException) { }

        public NoPredicateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class CancellationRequestedException : Exception
    {
        public CancellationRequestedException() :this("The requested operation cannot be completed because a cancellation has been requested.") { }

        public CancellationRequestedException(string message) : base(message) { }

        public CancellationRequestedException(string message, Exception innerException) : base(message, innerException) { }

        public CancellationRequestedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class StartException : Exception
    {
        public StartException() { }

        public StartException(string message) : base(message) { }

        public StartException(string message, Exception innerException) : base(message, innerException) { }

        public StartException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class NotRunningException : Exception
    {
        public NotRunningException() { }

        public NotRunningException(string message) : base(message) { }

        public NotRunningException(string message, Exception innerException) : base(message, innerException) { }

        public NotRunningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
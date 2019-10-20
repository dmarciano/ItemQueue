using System;

namespace SMC.Utilities.Queues
{
    public class ActionErrorEventArgs<T> : EventArgs
    {
        public T Data { get; set; }
        public string Message { get; set; }
        public Exception Error { get; set; }
        public string StackTrace => null != Error ? Error.StackTrace : string.Empty;

        public ActionErrorEventArgs(T data, string message)
        {
            Data = data;
            Message = message;
            Error = null;
        }

        public ActionErrorEventArgs(T data, string message, Exception error)
        {
            Data = data;
            Message = message;
            Error = error;
        }

        public ActionErrorEventArgs(T data, Exception error)
        {
            Data = data;
            Message = error.Message;
            Error = error;
        }
    }
}
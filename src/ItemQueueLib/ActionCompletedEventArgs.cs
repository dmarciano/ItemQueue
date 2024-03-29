﻿using System;

namespace SMC.Utilities.Queues
{
    public class ActionCompletedEventArgs<T> : EventArgs
    {
        public T Data { get; set; }
        public bool CompletedSuccessfully { get; set; }
        public string Message { get; set; }

        public ActionCompletedEventArgs(T data, bool completedSuccessfully, string message = "")
        {
            Data = data;
            CompletedSuccessfully = completedSuccessfully;
            Message = message;
        }
    }
}
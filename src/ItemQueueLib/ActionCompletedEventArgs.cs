using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
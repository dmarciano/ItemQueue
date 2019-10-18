using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC.Utilities.Queues
{
    public enum QueueStatus
    {
        Initialized,
        Stopped,
        Starting,
        Loading,
        Loaded,
        Running,
        Cancelling,
        Stopping,
        Disposing,
        Disposed
    }
}
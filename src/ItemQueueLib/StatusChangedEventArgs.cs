using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC.Utilities.Queues
{
    public class StatusChangedEventArgs : EventArgs
    {
        public QueueStatus Status { get; set; }

        public StatusChangedEventArgs(QueueStatus status)
        {
            Status = status;
        }
    }
}
using System;

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
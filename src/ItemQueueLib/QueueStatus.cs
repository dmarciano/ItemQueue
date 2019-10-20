namespace SMC.Utilities.Queues
{
    public enum QueueStatus
    {
        /// <summary>
        /// Queue has been initialized.
        /// </summary>
        Initialized,
        /// <summary>
        /// Queue is currently stopped.
        /// </summary>
        Stopped,
        /// <summary>
        /// Queue is starting up.
        /// </summary>
        Starting,
        /// <summary>
        /// Queue is processing data.
        /// </summary>
        Processing,
        /// <summary>
        /// Queue is waiting for items to process.
        /// </summary>
        Waiting,
        /// <summary>
        /// Cancellation of processing has been requested.
        /// </summary>
        Cancelling,
        /// <summary>
        /// Queue is stopping.
        /// </summary>
        Stopping,
        /// <summary>
        /// An exception has been encountered and the queue cannot continue.
        /// </summary>
        /// <remarks>See the <c>Error</c> property of the queue instance for the specific exception.</remarks>
        Error,
        /// <summary>
        /// Queue is being disposed.
        /// </summary>
        Disposing,
        /// <summary>
        /// Queue has been disposed.
        /// </summary>
        Disposed
    }
}
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SMC.Utilities.Queues
{
    public class ActionQueue<T> : IDisposable
    {
        #region Events
        public event EventHandler<StatusChangedEventArgs> StatusChanged;
        public event EventHandler<ActionCompletedEventArgs<T>> ActionCompleted;
        public event EventHandler<ActionErrorEventArgs<T>> ErrorOccurred;
        #endregion

        #region Variables
        private readonly ConcurrentQueue<T> _Queue;
        private readonly ManualResetEventSlim _ItemsAvailable;
        private Action<T> _Action = null;
        private Action<T> _Callback = null;
        private QueueStatus _Status;
        private bool _CancellationRequest = false;
        #endregion

        #region Properties
        /// <summary>
        /// The name of the queue.
        /// </summary>
        /// <remarks>If no name was specified when the queue was created, a random <see cref="Guid"/> is assigned as the queue's name.</remarks>
        public string Name { get; }
        /// <summary>
        /// The current status of the queue.  See <see cref="QueueStatus"/>.
        /// </summary>
        public QueueStatus Status
        {
            get
            {
                return _Status;
            }
            private set
            {
                _Status = value;
                OnStatusChanged(value);
            }
        }
        /// <summary>
        /// Indicates whether the queue has been stopped.  See <see cref="Stop(bool)"/>.
        /// </summary>
        /// <value><c>true</c> is the queue is stopping; otherwise <c>false</c>.</value>
        public bool CancellationRequest
        {
            get
            {
                return _CancellationRequest;
            }
            private set
            {
                _CancellationRequest = value;
                if (value) Status = QueueStatus.Cancelling;
            }
        }
        /// <summary>
        /// Total number of unprocessed items currently in the queue.
        /// </summary>
        public int ItemsInQueue => _Queue.Count;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new action queue.
        /// </summary>
        public ActionQueue() : this(null, null, string.Empty) { }

        /// <summary>
        /// Create a new action queue with the specific name.
        /// </summary>
        /// <param name="queueName">The name for this queue.</param>
        /// <remarks>The <paramref name="queueName"/> is also used as the name of the background thread.</remarks>
        public ActionQueue(string queueName) : this(null, null, queueName) { }

        /// <summary>
        /// Create a new action queue with the specified action.
        /// </summary>
        /// <param name="action">The action to call on each item in the queue.</param>
        public ActionQueue(Action<T> action) : this(action, null, string.Empty) { }

        /// <summary>
        /// Create a new action queue with the specified action and callback.
        /// </summary>
        /// <param name="action">The action to call on each item in the queue.</param>
        /// <param name="callback">The callback that will be called for each item that is successfully processed without an exception.</param>
        public ActionQueue(Action<T> action, Action<T> callback) : this(action, callback, string.Empty) { }

        /// <summary>
        /// Create a new action queue with the specified action and queue name.
        /// </summary>
        /// <param name="action">The action to call on each item in the queue.</param>
        /// <param name="queueName">The name for this queue.</param>
        /// <remarks>The <paramref name="queueName"/> is also used as the name of the background thread.</remarks>
        public ActionQueue(Action<T> action, string queueName) : this(action, null, queueName) { }

        /// <summary>
        /// Create a new action queue with the specified action, callback, and queue name.
        /// </summary>
        /// <param name="action">The action to call on each item in the queue.</param>
        /// <param name="callback">The callback that will be called for each item that is successfully processed without an exception.</param>
        /// <param name="queueName">The name for this queue.</param>
        /// <remarks>The <paramref name="queueName"/> is also used as the name of the background thread.</remarks>
        public ActionQueue(Action<T> action, Action<T> callback, string queueName)
        {
            _Action = action;
            _Callback = callback;
            Name = string.IsNullOrWhiteSpace(queueName) ? Guid.NewGuid().ToString("N").ToUpper() : queueName.Trim();

            _Queue = new ConcurrentQueue<T>();
            _ItemsAvailable = new ManualResetEventSlim(false);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starts the queue to begin processing items.
        /// </summary>
        /// <exception cref="NoActionException">No action was specified in the constructor or using the <see cref="SetAction(Action{T})"/> method.</exception>
        public void Start()
        {
            if (null == _Action) throw new NoActionException("An action must be specified before the queue can start processing items.");

            //TODO: Start processing
        }

        /// <summary>
        /// Stops the queue from processing items.
        /// </summary>
        /// <param name="processRemainingJob">Set to <c>true</c> to process any remaining jobs in the queue; <c>false</c> to discard all remaining items.</param>
        public void Stop(bool processRemainingJob)
        {
            //TODO: Stop processing
        }

        /// <summary>
        /// Sets the action called for each item in the queue.
        /// </summary>
        /// <param name="action">The action to call on each item in the queue.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="action"/> parameter is <c>null</c>.</exception>
        public void SetAction(Action<T> action)
        {
            _Action = action ?? throw new ArgumentNullException(nameof(action), "Action cannot be null.");
        }

        /// <summary>
        /// Sets the callback action.
        /// </summary>
        /// <param name="callback">The callback that will be called for each item that is successfully processed without an exception.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="callback"/> parameter is <c>null</c>.</exception>
        public void SetCallback(Action<T> callback)
        {
            _Callback = callback ?? throw new ArgumentNullException(nameof(callback), "Callback cannot be null.  If no callback should be used, call the StopCallbacks method.");
        }

        /// <summary>
        /// Prevents the callback action from being executed.
        /// </summary>
        /// <remarks>To enable callbacks again, call the <see cref="SetCallback(Action{T})"/> method.</remarks>
        public void StopCallbacks()
        {
            _Callback = null;
        }
        #endregion

        #region Event Method
        protected virtual void OnStatusChanged(QueueStatus newStatus)
        {
            OnStatusChanged(new StatusChangedEventArgs(newStatus));
        }

        protected virtual void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        protected virtual void OnActionCompleted(T data, bool completedSuccessfully, string message = "")
        {
            OnActionCompleted(new ActionCompletedEventArgs<T>(data, completedSuccessfully, message));
        }

        protected virtual void OnActionCompleted(ActionCompletedEventArgs<T> e)
        {
            ActionCompleted?.Invoke(this, e);
        }

        protected virtual void OnErrorOccurred(T data, string message)
        {
            OnErrorOccurred(new ActionErrorEventArgs<T>(data, message));
        }

        protected virtual void OnErrorOccurred(T data, string message, Exception ex)
        {
            OnErrorOccurred(new ActionErrorEventArgs<T>(data, message, ex));
        }

        protected virtual void OnErrorOccurred(T data, Exception ex)
        {
            OnErrorOccurred(new ActionErrorEventArgs<T>(data, ex));
        }

        protected virtual void OnErrorOccurred(ActionErrorEventArgs<T> e)
        {
            ErrorOccurred?.Invoke(this, e);
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Status = QueueStatus.Disposing;
                if (disposing)
                {
                    Stop(true);
                    // TODO: dispose managed state (managed objects).
                }
                Status = QueueStatus.Disposed;
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ActionQueue() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
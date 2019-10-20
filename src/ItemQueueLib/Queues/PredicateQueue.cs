using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace SMC.Utilities.Queues
{
    public class PredicateQueue<T> : IDisposable
    {
        #region Events
        public event EventHandler<StatusChangedEventArgs> StatusChanged;
        public event EventHandler<ActionCompletedEventArgs<T>> ActionCompleted;
        public event EventHandler<ActionErrorEventArgs<T>> ErrorOccurred;
        #endregion

        #region Variables
        private readonly ConcurrentQueue<T> _Queue;
        private readonly ManualResetEventSlim _ItemsAvailable;
        private readonly ManualResetEventSlim _Finished;
        private Predicate<T> _Predicate;
        private Action<T> _Action;
        private Action<T> _Callback;
        private QueueStatus _Status;
        private bool _CancellationRequested;
        private Thread _ProcessingThread;
        private bool _ProcessRemainingItems;
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
            get => _Status;
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
        public bool CancellationRequested
        {
            get => _CancellationRequested;
            private set
            {
                _CancellationRequested = value;
                if (value) Status = QueueStatus.Cancelling;
            }
        }

        /// <summary>
        /// Total number of unprocessed items currently in the queue.
        /// </summary>
        public int ItemsInQueue => _Queue.Count;

        /// <summary>
        /// The current exception which is preventing the queue from processing more items.
        /// </summary>
        public Exception Error { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new predicate queue.
        /// </summary>
        public PredicateQueue() : this(null, null, string.Empty) { }

        /// <summary>
        /// Create a new predicate queue with the specific name.
        /// </summary>
        /// <param name="queueName">The name for this queue.</param>
        /// <remarks>The <paramref name="queueName"/> is also used as the name of the background thread.</remarks>
        public PredicateQueue(string queueName) : this(null, null, queueName) { }

        /// <summary>
        /// Create a new predicate queue with the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate that defines the conditions of the element to call the <paramref name="action"/> on.</param>
        /// <param name="action">The action to call on each item in the queue.</param>
        public PredicateQueue(Predicate<T> predicate, Action<T> action) : this(predicate, action, null, string.Empty) { }

        /// <summary>
        /// Create a new predicate queue with the specified predicate, action, and callback.
        /// </summary>
        /// <param name="predicate">The predicate that defines the conditions of the element to call the <paramref name="action"/> on.</param>
        /// <param name="action">The action to call on each item in the queue.</param>
        /// <param name="callback">The callback that will be called for each item that is successfully processed without an exception.</param>
        public PredicateQueue(Predicate<T> predicate, Action<T> action, Action<T> callback) : this(predicate, action, callback, string.Empty) { }

        /// <summary>
        /// Create a new predicate queue with the specified predicate, action, and queue name.
        /// </summary>
        /// <param name="predicate">The predicate that defines the conditions of the element to call the <paramref name="action"/> on.</param>
        /// <param name="action">The action to call on each item in the queue.</param>
        /// <param name="queueName">The name for this queue.</param>
        /// <remarks>The <paramref name="queueName"/> is also used as the name of the background thread.</remarks>
        public PredicateQueue(Predicate<T> predicate, Action<T> action, string queueName) : this(predicate, action, null, queueName) { }

        /// <summary>
        /// Create a new predicate queue with the specified predicate, action, callback, and queue name.
        /// </summary>
        /// <param name="predicate">The predicate that defines the conditions of the element to call the <paramref name="action"/> on.</param>
        /// <param name="action">The action to call on each item in the queue.</param>
        /// <param name="callback">The callback that will be called for each item that is successfully processed without an exception.</param>
        /// <param name="queueName">The name for this queue.</param>
        /// <remarks>The <paramref name="queueName"/> is also used as the name of the background thread.</remarks>
        public PredicateQueue(Predicate<T> predicate, Action<T> action, Action<T> callback, string queueName)
        {
            _Predicate = predicate;
            _Action = action;
            _Callback = callback;
            Name = string.IsNullOrWhiteSpace(queueName) ? Guid.NewGuid().ToString("N").ToUpper() : queueName.Trim();

            _Queue = new ConcurrentQueue<T>();
            _ItemsAvailable = new ManualResetEventSlim(false);
            _Finished = new ManualResetEventSlim(false);
            Status = QueueStatus.Initialized;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starts the queue to begin processing items.
        /// </summary>
        /// <returns><c>true</c> is the queue was successfully started, otherwise <c>false</c>.</returns>
        /// <remarks>The queue will be run on a background thread with normal priority.</remarks>
        /// <exception cref="NoPredicateException">No predicate was specified in the constructor or using the <see cref="SetPredicate(Predicate{T})"/> method.</exception>
        /// <exception cref="NoActionException">No action was specified in the constructor or using the <see cref="SetAction(Action{T})"/> method.</exception>
        /// <exception cref="CancellationRequestedException">The queue is in the process of being canceled.</exception>
        /// <exception cref="StartException">The queue could not be started.  See the exception message property for the exact reason.</exception>
        public bool Start()
        {
            return Start(true, ThreadPriority.Normal);
        }

        /// <summary>
        /// Starts the queue to begin processing items.
        /// </summary>
        /// <param name="isBackground"><c>true</c> is the queue should process items on a background thread, otherwise <c>false</c>.</param>
        /// <returns><c>true</c> is the queue was successfully started, otherwise <c>false</c>.</returns>
        /// <remarks>The queue will be run with normal priority.</remarks>
        /// <exception cref="NoPredicateException">No predicate was specified in the constructor or using the <see cref="SetPredicate(Predicate{T})"/> method.</exception>
        /// <exception cref="NoActionException">No action was specified in the constructor or using the <see cref="SetAction(Action{T})"/> method.</exception>
        /// <exception cref="CancellationRequestedException">The queue is in the process of being canceled.</exception>
        /// <exception cref="StartException">The queue could not be started.  See the exception message property for the exact reason.</exception>
        public bool Start(bool isBackground)
        {
            return Start(isBackground, ThreadPriority.Normal);
        }

        /// <summary>
        /// Starts the queue to begin processing items.
        /// </summary>
        /// <param name="priority">The <see cref="ThreadPriority"/> to run the background thread at.</param>
        /// <returns><c>true</c> is the queue was successfully started, otherwise <c>false</c>.</returns>
        /// <remarks>The queue will be run on a background thread.</remarks>
        /// <exception cref="NoPredicateException">No predicate was specified in the constructor or using the <see cref="SetPredicate(Predicate{T})"/> method.</exception>
        /// <exception cref="NoActionException">No action was specified in the constructor or using the <see cref="SetAction(Action{T})"/> method.</exception>
        /// <exception cref="CancellationRequestedException">The queue is in the process of being canceled.</exception>
        /// <exception cref="StartException">The queue could not be started.  See the exception message property for the exact reason.</exception>
        public bool Start(ThreadPriority priority)
        {
            return Start(true, priority);
        }

        /// <summary>
        /// Starts the queue to begin processing items.
        /// </summary>
        /// <param name="isBackground"><c>true</c> is the queue should process items on a background thread, otherwise <c>false</c>.</param>
        /// <param name="priority">The <see cref="ThreadPriority"/> to run the background thread at.</param>
        /// <returns><c>true</c> is the queue was successfully started, otherwise <c>false</c>.</returns>
        /// <exception cref="NoPredicateException">No predicate was specified in the constructor or using the <see cref="SetPredicate(Predicate{T})"/> method.</exception>
        /// <exception cref="NoActionException">No action was specified in the constructor or using the <see cref="SetAction(Action{T})"/> method.</exception>
        /// <exception cref="CancellationRequestedException">The queue is in the process of being canceled.</exception>
        /// <exception cref="StartException">The queue could not be started.  See the exception message property for the exact reason.</exception>
        public bool Start(bool isBackground, ThreadPriority priority)
        {
            if(null == _Predicate) throw new NoPredicateException("A predicate must be specified before the queue can start processing items.");
            if (null == _Action) throw new NoActionException("An action must be specified before the queue can start processing items.");
            if (CancellationRequested) throw new CancellationRequestedException("The queue cannot be started at this time because it is in the process of cancelling.");
            if (QueueStatus.Initialized != Status && QueueStatus.Stopped != Status) throw new StartException("The queue must be either in the Initialize or Stopped state to be started again.");
            try
            {
                Status = QueueStatus.Starting;
                _ProcessingThread = new Thread(Run) { Name = Name, IsBackground = isBackground, Priority = priority };
                _ProcessingThread.Start();
                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(default(T), "An exception occurred while starting the queue.", ex);
                return false;
            }
        }

        /// <summary>
        /// Stops the queue from processing items.
        /// </summary>
        /// <param name="processRemainingItems">Set to <c>true</c> to process any remaining items in the queue; <c>false</c> to discard all remaining items.</param>
        /// <returns><c>true</c> is the stop request was successfully received and processed, otherwise <c>false</c>.</returns>
        public bool Stop(bool processRemainingItems)
        {
            try
            {
                if (CancellationRequested
                    || QueueStatus.Initialized == Status
                    || QueueStatus.Stopping == Status
                    || QueueStatus.Stopped == Status
                    || QueueStatus.Disposing == Status
                    || QueueStatus.Disposed == Status) return true;

                Status = QueueStatus.Stopping;
                CancellationRequested = true;
                _ProcessRemainingItems = processRemainingItems;
                _ItemsAvailable.Set();
                _Finished.Wait();
                _Finished.Reset();
                Status = QueueStatus.Stopped;
                CancellationRequested = false;
                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(default(T), "An exception occurred while stopping the queue.", ex);
                return false;
            }
        }

        /// <summary>
        /// Set the predicate used to determine if the action should be called for the specific item being processed.
        /// </summary>
        /// <param name="predicate">The predicate that defines the conditions of the element to call the action on.</param>
        public void SetPredicate(Predicate<T> predicate)
        {
            _Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate), "Predicate cannot be null.");
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

        /// <summary>
        /// Enqueue item for processing.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        public void Enqueue(T item)
        {
            if (QueueStatus.Initialized != Status && QueueStatus.Starting != Status && QueueStatus.Processing != Status && QueueStatus.Waiting != Status) throw new NotRunningException($"Queue must be in the Processing or Waiting state in order to enqueue new items. (Current State: {Status.ToString()})");
            try
            {
                _Queue.Enqueue(item);
                _ItemsAvailable.Set();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(item, "An exception occurred while attempting to enqueue an item.", ex);
            }
        }

        /// <summary>
        /// Enqueue a multiple items at once.
        /// </summary>
        /// <param name="items">The <see cref="List{T}"/> of items to enqueue.</param>
        public void Enqueue(List<T> items)
        {
            if (QueueStatus.Initialized != Status && QueueStatus.Starting != Status && QueueStatus.Processing != Status && QueueStatus.Waiting != Status) throw new NotRunningException($"Queue must be in the Processing or Waiting state in order to enqueue new items. (Current State: {Status.ToString()})");
            var item = default(T);
            try
            {
                foreach (var i in items)
                {
                    item = i;
                    _Queue.Enqueue(item);
                }

                _ItemsAvailable.Set();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(item, "An exception occurred while attempting to enqueue items.", ex);
            }
        }

        private void Run()
        {
            Status = QueueStatus.Processing;
            try
            {
                while (!CancellationRequested)
                {
                    if (_Queue.Count < 1)
                        _ItemsAvailable.Reset();
                    else
                        ProcessItems();

                    Status = QueueStatus.Waiting;
                    _ItemsAvailable.Wait();
                }

                ProcessItems();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(default(T), "An exception occurred in main processing thread.", ex);
            }
        }

        private void ProcessItems()
        {
            Status = QueueStatus.Processing;
            var currentItem = default(T);
            try
            {
                if (CancellationRequested)
                {
                    if (_ProcessRemainingItems)
                    {
                        foreach (var i in _Queue)
                        {
                            if (_Queue.TryDequeue(out currentItem))
                            {
                                if (_Predicate(currentItem))
                                {
                                    _Action(currentItem);
                                    OnActionCompleted(currentItem, true, string.Empty);
                                    _Callback?.Invoke(currentItem);
                                }
                                else
                                {
                                    OnActionCompleted(currentItem, false, $"The predicate returned false for item '{currentItem}'.");
                                }
                            }
                            else
                            {
                                OnErrorOccurred(currentItem, "An exception occurred while dequeuing an item for processing.");
                                OnActionCompleted(currentItem, false, "An exception occurred while dequeuing an item for processing.");
                            }
                        }
                    }
                    else
                    {
                        while (!_Queue.IsEmpty) _Queue.TryDequeue(out var _);
                    }

                    _Finished.Set();
                }
                else
                {
                    foreach (var i in _Queue)
                    {
                        if (_Queue.TryDequeue(out currentItem))
                        {
                            if (_Predicate(currentItem))
                            {
                                _Action(currentItem);
                                OnActionCompleted(currentItem, true, string.Empty);
                                _Callback?.Invoke(currentItem);
                            }
                            else
                            {
                                OnActionCompleted(currentItem, false, $"The predicate returned false for item '{currentItem}'.");
                            }
                        }
                        else
                        {
                            OnErrorOccurred(currentItem, "An exception occurred while dequeuing an item for processing.");
                            OnActionCompleted(currentItem, false, "An exception occurred while dequeuing an item for processing.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred(currentItem, $"An exception occurred while processing item '{currentItem}'.", ex);
                OnActionCompleted(currentItem, false, $"An exception occurred while processing item '{currentItem}'.");
            }
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
            Status = QueueStatus.Error;
            Error = e.Error;
            ErrorOccurred?.Invoke(this, e);
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Status = QueueStatus.Disposing;
                if (disposing)
                {
                    Stop(true);
                }
                Status = QueueStatus.Disposed;
                _ProcessingThread.Join();
                _ProcessingThread = null;

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
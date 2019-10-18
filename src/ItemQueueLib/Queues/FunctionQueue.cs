using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace SMC.Utilities.Queues
{
    public class FunctionQueue<T, TResult> : IDisposable
    {
        #region Variables
        private readonly ConcurrentQueue<T> _Queue;
        private readonly ManualResetEventSlim _ItemsAvailable;
        private Func<T, TResult> _Function;
        private Action<TResult> _Callback;
        private bool _CancellationRequest = false;
        #endregion

        #region Properties
        public QueueStatus Status { get; set; }
        public bool CancellationRequest => _CancellationRequest;
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FunctionQueue() {
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

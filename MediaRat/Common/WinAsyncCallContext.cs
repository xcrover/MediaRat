using System;
using System.Windows;
using System.Threading;
using System.Windows.Threading;

namespace Ops.NetCoe.LightFrame {

    /// <summary>
    /// Async call context for Silverlight
    /// </summary>
    public class WinAsyncCallContext : IAsyncCallContext {
        #region Private Members
        private SynchronizationContext _syncContext;
        ///<summary>Dispatcher</summary>
        private Dispatcher _dispatcher;
        ///<summary>Tag 1</summary>
        private object _tag1;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="WinAsyncCallContext"/> class.
        /// </summary>
        public WinAsyncCallContext() {
            SaveThreadContext();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinAsyncCallContext"/> class.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="asyncState">State of the async.</param>
        public WinAsyncCallContext(object operation, object asyncState)
            : this() {
            this.Operation = operation;
            this.AsyncState = asyncState;
        }
        #region IAsyncCallContext Members

        /// <summary>
        /// User specified Async State that is a usual part of the BeginOperation methods.
        /// </summary>
        /// <value></value>
        public object AsyncState {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the additional user resources that should be disposed when this context is disposed.
        /// </summary>
        /// <value>The tag.</value>
        public IDisposable DisposableResources {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the call finalizer, e.g. method EndBlahBlahBlah in WCF service proxy that should be called to
        /// complete the call to the external service.
        /// </summary>
        /// <value>The finalize call.</value>
        public AsyncCallback EndCall {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the abort call delegate.
        /// </summary>
        /// <value>The abort call.</value>
        public AbortAsyncOperationAction AbortCall {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the operation information (e.g. operation Id).
        /// </summary>
        /// <value>The operation.</value>
        public object Operation {
            get;
            set;
        }

        /// <summary>Exception that happened during the invocation if any</summary>
        public Exception Error { get; set; }

        /// <summary>
        /// Restores the thread context that was build by <see cref="SaveThreadContext"/> for the current thread
        /// after the asynchronous call.
        /// </summary>
        public void RestoreThreadContext() {
        }

        /// <summary>
        /// Build the internal representation of the current thread context, e.g. save HttpContext.Current,
        /// Principal and Culture.
        /// </summary>
        public void SaveThreadContext() {
            this._syncContext = SynchronizationContext.Current;
            this._dispatcher = Dispatcher.CurrentDispatcher;
        }

        #endregion


        /// <summary>
        /// Gets the initial synchronization context.
        /// </summary>
        /// <value>The initial sync context.</value>
        public SynchronizationContext InitialSyncContext {
            get { return this._syncContext; }
        }

        ///<summary>Dispatcher</summary>
        public Dispatcher Dispatcher {
            get { return this._dispatcher; }
        }

        ///<summary>Tag 1</summary>
        public object Tag1 {
            get { return this._tag1; }
            set { this._tag1 = value; }
        }

        /// <summary>
        /// Tag 1 as <typeparamref name="Tcast"/>.
        /// </summary>
        /// <typeparam name="Tcast">The type of the cast.</typeparam>
        /// <returns>Casted instance</returns>
        public Tcast Tag1As<Tcast>() where Tcast : class {
            return Tag1 as Tcast;
        }

        #region IDisposable Members

        public void Dispose() {
            throw new NotImplementedException();
        }

        #endregion

    }
}

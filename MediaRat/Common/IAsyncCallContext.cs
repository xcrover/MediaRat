using System;

namespace Ops.NetCoe.LightFrame {

    /// <summary>
    /// Delegate that defines signature for the initiator of the asynchronous call
    /// </summary>
    /// <param name="callContext">Context information of the asynchronous method invocation</param>
    /// <param name="callBack">Method to be called when operation is completed.</param>
    public delegate IAsyncResult BeginAsyncOperationAction(IAsyncCallContext callContext, AsyncCallback callBack);

    /// <summary>
    /// Delagate that defines signature for the <c>Abort Execution</c> method.
    /// </summary>
    /// <param name="callContext">Context information of the asynchronous method invocation</param>
    /// <returns>True if operation aborted; otherwise - false.</returns>
    public delegate bool AbortAsyncOperationAction(IAsyncCallContext callContext);

    /// <summary>
    /// Context information of the asynchronous method invocation.
    /// </summary>
    public interface IAsyncCallContext : IDisposable {
        /// <summary>
        /// User specified Async State that is a usual part of the BeginOperation methods.
        /// </summary>
        object AsyncState { get; set; }

        /// <summary>
        /// Gets or sets the tag1.
        /// </summary>
        /// <value>
        /// The tag1.
        /// </value>
        object Tag1 { get; set; }

        /// <summary>
        /// Gets or sets the additional user resources that should be disposed when this context is disposed.
        /// </summary>
        /// <value>The tag.</value>
        IDisposable DisposableResources { get; set; }

        /// <summary>
        /// Gets or sets the call finalizer, e.g. method EndBlahBlahBlah in WCF service proxy that should be called to
        /// complete the call to the external service.
        /// </summary>
        /// <value>The finalize call.</value>
        AsyncCallback EndCall { get; set; }

        /// <summary>
        /// Gets or sets the abort call delegate.
        /// </summary>
        /// <value>The abort call.</value>
        AbortAsyncOperationAction AbortCall { get; set; }

        /// <summary>
        /// Gets or sets the operation information (e.g. operation Id).
        /// </summary>
        /// <value>The operation.</value>
        object Operation { get; set; }

        /// <summary>
        /// Build the internal representation of the current thread context, e.g. save HttpContext.Current, 
        /// Principal and Culture.
        /// </summary>
        void SaveThreadContext();

        /// <summary>
        /// Restores the thread context that was build by <see cref="SaveThreadContext"/> for the current thread
        /// after the asynchronous call.
        /// </summary>
        void RestoreThreadContext();

    }


    /// <summary>
    /// Standard Extensions
    /// </summary>
    public static class IAsyncCallContextExtensions {

        /// <summary>
        /// Tag 1 as <paramref name="Tcast"/>.
        /// </summary>
        /// <typeparam name="Tcast">The type of the cast.</typeparam>
        /// <param name="actx">The actx.</param>
        /// <returns></returns>
        public static Tcast Tag1As<Tcast>(this IAsyncCallContext actx) where Tcast : class {
            return actx.Tag1 as Tcast;
        }
    }
}

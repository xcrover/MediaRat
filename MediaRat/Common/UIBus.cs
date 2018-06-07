using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using Ops.NetCoe.LightFrame;
using System.Threading;
using System.Collections.Generic;

namespace XC.MediaRat {

    /// <summary>
    /// Message types for UI bus <see cref="IApplicationBus"/>
    /// </summary>
    public enum UIBusMessageTypes {
        /// <summary>Undefined</summary>
        Undefined = 0,
        /// <summary>Undefined</summary>
        ChangeNotification = 1,
        /// <summary>Action request, e.g. edit entity</summary>
        ActionRequest = 2,
        /// <summary>Request information from user</summary>
        InformationRequest = 3,
        /// <summary>Navigate To</summary>
        NavigationRequest = 8,
        /// <summary>Change of the current user profile</summary>
        CurrentUserProfileChanged = 9,
    }

    /// <summary>
    /// Request for action
    /// </summary>
    public class ActionRequest {

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionRequest"/> class.
        /// </summary>
        public ActionRequest() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionRequest"/> class.
        /// </summary>
        /// <param name="operationId">The operation id.</param>
        /// <param name="subject">The subject.</param>
        public ActionRequest(UIOperations operationId, object subject) {
            this.OperationId = operationId;
            this.Subject = subject;
        }

        /// <summary>
        /// Gets or sets the operation id.
        /// </summary>
        /// <value>The operation id.</value>
        public UIOperations OperationId { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>The subject.</value>
        public object Subject { get; set; }

    }


    /// <summary>
    /// Request to navigate to another form
    /// </summary>
    public class NavigationRequest {
        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        /// <value>The target.</value>
        public UITargets Target { get; set; }
        /// <summary>
        /// Gets or sets the target id.
        /// </summary>
        /// <value>The target id.</value>
        public int TargetId { get; set; }
        /// <summary>
        /// Gets or sets the target work space. If this parameter specified then other Target specifiers are not used.
        /// </summary>
        /// <value>The target work space.</value>
        public WorkspaceViewModel TargetWorkSpace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether target needs refresh.
        /// </summary>
        /// <value><c>true</c> if target needs refresh; otherwise, <c>false</c>.</value>
        public bool NeedRefresh { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public object Data { get; set; }
    }

    /// <summary>
    /// Change notification payload for the Application Bus
    /// </summary>
    public class ChangeNotification {

        /// <summary>
        /// Gets or sets the operation id.
        /// </summary>
        /// <value>The operation id.</value>
        public UIOperations OperationId { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>The subject.</value>
        public object Subject { get; set; }
    }

    /// <summary>
    /// Information request payload for the ApplicationBus
    /// </summary>
    public class InformationRequest {

        /// <summary>
        /// Gets or sets the type of the result.
        /// </summary>
        /// <value>The type of the result.</value>
        public Type ResultType { get; set; }

        /// <summary>
        /// Gets or sets the complete delegate that is called.
        /// </summary>
        /// <value>The complete method.</value>
        public Action<InformationRequest> CompleteMethod { get; set; }

        /// <summary>
        /// Gets or sets the prompt.
        /// </summary>
        /// <value>The prompt.</value>
        public object Prompt { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>The result.</value>
        public object Result { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is canceled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is canceled; otherwise, <c>false</c>.
        /// </value>
        public bool IsCanceled { get; set; }

        /// <summary>
        /// Gets or sets the result max length of result if applicable.
        /// </summary>
        /// <value>The result max len.</value>
        public int ResultMaxLen { get; set; }

        /// <summary>
        /// Gets or sets the validation method.
        /// </summary>
        /// <value>The validation method.</value>
        public Func<object, string> ValidationMethod { get; set; }

        /// <summary>
        /// Gets the error text. Returns empty string if there are no errors.
        /// </summary>
        /// <value>The error.</value>
        public virtual string Error {
            get { return (ValidationMethod == null) ? string.Empty : ValidationMethod(this.Result); }
        }

        /// <summary>
        /// Additional information
        /// </summary>
        public object Tag { get; set; }
    }

    /// <summary>
    /// Request to access file via Open or Save File dialog
    /// </summary>
    public class FileAccessRequest {

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAccessRequest"/> class.
        /// </summary>
        public FileAccessRequest() {
            SuggestedFileName = string.Empty;
            ExtensionFilter = "All files (*.*)|*.*";
        }

        /// <summary><c>True</c> if access requested for reading; otherwise for writing.</summary>
        public bool IsForReading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether more than one file can be selected for reading.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if more than one file can be selected for reading; otherwise, <c>false</c>.
        /// </value>
        public bool IsMultiSelect { get; set; }

        /// <summary>
        /// Gets or sets the suggested file name. Can be <c>null</c> or empty.
        /// </summary>
        /// <value>The suggested file name.</value>
        public string SuggestedFileName { get; set; }

        /// <summary>
        /// Gets or sets the extension filter in the same format as it is used in the Open|SaveFileDialog:
        /// <para>"XML documents (*.xml)|*.xml|Text documents (*.txt)|*.txt|All files (*.*)|*.*"</para>
        /// </summary>
        /// <value>The extension filter.</value>
        public string ExtensionFilter { get; set; }

        /// <summary>
        /// Gets or sets the <c>1-based</c> index of the default entry in the extension filter.
        /// </summary>
        /// <value>The index of the default entry in the extension filter.</value>
        public int ExtensionFilterIndex { get; set; }

    }



    /// <summary>
    /// Contract for the intra-application messaging bus.
    /// This notification system is used to decouple differnet subsystems.
    /// </summary>
    public interface IApplicationBus<TMessageType> {

        /// <summary>
        /// Adds the subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        void AddSubscriber(IApplicationBusSubscriber<TMessageType> subscriber);

        /// <summary>
        /// Removes the subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        void RemoveSubscriber(IApplicationBusSubscriber<TMessageType> subscriber);

        /// <summary>
        /// Sends the specified message and wait untill all the subscribers are completed.
        /// </summary>
        /// <param name="message">The message.</param>
        void Send(ApplicationMessage<TMessageType> message);

        /// <summary>
        /// Posts the specified message on the background thread and continue execution.
        /// </summary>
        /// <param name="message">The message.</param>
        void Post(ApplicationMessage<TMessageType> message);
    }

    /// <summary>
    /// Application Bus subscriber
    /// </summary>
    public interface IApplicationBusSubscriber<TMessageType> {
        /// <summary>
        /// Called when message is delivered.
        /// </summary>
        /// <param name="message">The message.</param>
        void OnMessage(ApplicationMessage<TMessageType> message);
    }

    /// <summary>
    /// Application message
    /// </summary>
    public class ApplicationMessage<TMessageType> {

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationMessage"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="messageType">Type of the message.</param>
        public ApplicationMessage(object sender, TMessageType messageType) {
            this.Sender = sender;
            this.MessageType = messageType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationMessage"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="payload">The payload.</param>
        public ApplicationMessage(object sender, TMessageType messageType, object payload)
            : this(sender, messageType) {
            this.Payload = payload;
        }

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>The type of the message.</value>
        public TMessageType MessageType { get; protected set; }

        /// <summary>
        /// Gets or sets the sender.
        /// </summary>
        /// <value>The sender.</value>
        public object Sender { get; protected set; }

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// <value>The payload.</value>
        public object Payload { get; protected set; }

    }

    /// <summary>
    /// <see cref="IApplicationBus"/> implementation
    /// </summary>
    public class ApplicationBus<TMessageType> : IApplicationBus<TMessageType> {
        Dictionary<IApplicationBusSubscriber<TMessageType>, object> _subscribers = new Dictionary<IApplicationBusSubscriber<TMessageType>, object>(32);
        ReaderWriterLock _rwLock = new ReaderWriterLock();
        /// <summary>
        ///  Timeout in milliseconds
        /// </summary>
        const int lockTimeOut = 2000;

        #region IApplicationBus<TMessageType> Members

        /// <summary>
        /// Adds the subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        public void AddSubscriber(IApplicationBusSubscriber<TMessageType> subscriber) {
            this._rwLock.AcquireWriterLock(lockTimeOut);
            this._subscribers[subscriber] = null;
            this._rwLock.ReleaseWriterLock();
        }

        /// <summary>
        /// Removes the subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        public void RemoveSubscriber(IApplicationBusSubscriber<TMessageType> subscriber) {
            this._rwLock.AcquireWriterLock(lockTimeOut);
            if (this._subscribers.ContainsKey(subscriber))
                this._subscribers.Remove(subscriber);
            this._rwLock.ReleaseWriterLock();
        }

        /// <summary>
        /// Sends the specified message and wait untill all the subscribers are completed.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Send(ApplicationMessage<TMessageType> message) {
            List<IApplicationBusSubscriber<TMessageType>> targets = null;
            try {
                this._rwLock.AcquireReaderLock(lockTimeOut);
                if (this._subscribers.Keys.Count > 0) {
                    targets = new List<IApplicationBusSubscriber<TMessageType>>(
                        from sb in this._subscribers.Keys select sb
                        );
                }
            }
            finally {
                this._rwLock.ReleaseReaderLock();
            }
            if (targets != null) {
                foreach (var sbs in targets) {
                    sbs.OnMessage(message);
                }
            }
        }

        /// <summary>
        /// Posts the specified message on the background thread and continue execution.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Post(ApplicationMessage<TMessageType> message) {
            ThreadPool.QueueUserWorkItem((p) => this.Send(message));
        }

        #endregion
    }

    /// <summary>
    /// UI message bus
    /// </summary>
    public class UIBus : ApplicationBus<UIBusMessageTypes> { }

    /// <summary>
    /// UI Bus subscriber
    /// </summary>
    public interface IUIBusSubscriber : IApplicationBusSubscriber<UIBusMessageTypes> { }

    /// <summary>
    /// UI Bus message
    /// </summary>
    public class UIMessage : ApplicationMessage<UIBusMessageTypes> {
       /// <summary>
        /// Initializes a new instance of the <see cref="UIMessage"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="messageType">Type of the message.</param>
        public UIMessage(object sender, UIBusMessageTypes messageType) : base(sender, messageType) {  }

        /// <summary>
        /// Initializes a new instance of the <see cref="UIMessage"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="payload">The payload.</param>
        public UIMessage(object sender, UIBusMessageTypes messageType, object payload) : base(sender, messageType, payload) { }

    }

}

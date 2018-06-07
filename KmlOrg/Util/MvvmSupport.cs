using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Ops.NetCoe.LightFrame {

    /// <summary>
    /// Types of messages to user
    /// </summary>
    public enum UserMessageTypes {
        Undefined = 0,
        /// <summary>Neutral</summary>
        Neutral = 1,
        /// <summary>Positive</summary>
        Positive = 2,
        /// <summary>Warning</summary>
        Warning = 3,
        /// <summary>Error</summary>
        Error = 4
    }


    /// <summary>
    /// Presents message to the user
    /// </summary>
    public interface IMessagePresenter {
        /// <summary>
        /// Clears the message.
        /// </summary>
        void Clear();

        /// <summary>
        /// Sets the message.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="message">The message.</param>
         void SetMessage(UserMessageTypes messageType, object message);
    }

    /// <summary>
    /// Base class that implements <see cref="INotifyPropertyChanged"/>
    /// </summary>
    public class NotifyPropertyChangedBase : INotifyPropertyChanged {
        ///<summary>Flag that controls throwing an exception for invalid property name in FirePropertyChanged</summary>
        private bool _throwOnInvalidPropertyName;
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        ///<summary>Flag that controls throwing an exception for invalid property name in FirePropertyChanged</summary>
        public bool ThrowOnInvalidPropertyName {
            get { return this._throwOnInvalidPropertyName; }
            protected set { this._throwOnInvalidPropertyName = value; }
        }

        /// <summary>
        /// Fires the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public virtual void FirePropertyChanged(string propertyName) {
            VerifyPropertyName(propertyName);
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Verifies the name of the property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [Conditional("DEBUG")]
        //[DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName) {
            // Verify that the property name matches a real,  
            // public, instance property on this object.

            if (this.GetType().GetProperty(propertyName) == null) {
                string msg = "Invalid property name in " + this.GetType().FullName + ": " + propertyName;
                if (this.ThrowOnInvalidPropertyName)
                    AppContext.Current.LogTechError(msg, null);
                else
                    AppContext.Current.LogTechError(msg, null);
            }
        }

    }

    /// <summary>
    /// Base class for MVVM pattern View Model.
    /// </summary>
    public abstract class ViewModelBase : NotifyPropertyChangedBase, IDisposable, INotifyPropertyChanged {
        #region Private Members
        ///<summary>User-friendly Title</summary>
        private string _title;
        ///<summary>Is Busy flag signals when operation is being executed on this ViewModel</summary>
        private bool _isBusy;
        ///<summary>Thread Dispatcher for the UI thread. Automatically initialized to the thread on which this instance has been created.</summary>
        private Dispatcher _uiDispatcher = Dispatcher.CurrentDispatcher;


        #endregion

        #region Construction
        #endregion

        #region Properties

        ///<summary>User-friendly Title</summary>
        public virtual string Title {
            get { return this._title; }
            set {
                if (this._title != value) {
                    this._title = value;
                    FirePropertyChanged("Title");
                }
            }
        }

        ///<summary>Is Busy flag signals when operation is being executed on this ViewModel</summary>
        public bool IsBusy {
            get { return this._isBusy; }
            protected set {
                if (this._isBusy != value) {
                    this._isBusy = value;
                    this.OnIsBusyChanged(value);
                    FirePropertyChanged("IsBusy");
                }
            }
        }

        ///<summary>Thread Dispatcher for the UI thread. Automatically initialized to the thread on which this instance has been created.</summary>
        public Dispatcher UIDispatcher {
            get { return this._uiDispatcher; }
            set { this._uiDispatcher = value; }
        }

        #endregion

        #region Operations

        /// <summary>
        /// Called when <see cref="IsBusy"/> changed.
        /// </summary>
        /// <param name="newValue">New value of <see cref="IsBusy"/>.</param>
        protected virtual void OnIsBusyChanged(bool newValue) {
        }

        /// <summary>
        /// Called when instance is disposed. 
        /// Child classes can override this method to add their own logic, e.g. remove event handlers.
        /// </summary>
        protected virtual void OnDispose() {
        }

        /// <summary>
        /// Ensures that the specified operation is performed on UI thread.
        /// This method uses <see cref="UIDispatcher"/> to invoke the method.
        /// </summary>
        /// <param name="act">The act.</param>
        public void RunOnUIThread(Action act) {
            if (this.UIDispatcher.CheckAccess())
                act();
            else
                this.UIDispatcher.BeginInvoke(act, DispatcherPriority.Normal);
        }

        /// <summary>
        /// Fires the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        override public void FirePropertyChanged(string propertyName) {
            this.RunOnUIThread(() => base.FirePropertyChanged(propertyName));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ViewModelBase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
 
        #endregion

    }


    /// <summary>
    /// View model to present status to user
    /// </summary>
    public class StatusVModel : ViewModelBase, IMessagePresenter {
        ///<summary>Status</summary>
        private UserMessageTypes _status;
        ///<summary>User Message</summary>
        private object _userMessage;
        ///<summary>Has Message</summary>
        private bool _hasMessage;

        ///<summary>Has Message</summary>
        public bool HasMessage {
            get { return this._hasMessage; }
            set {
                if (this._hasMessage != value) {
                    this._hasMessage = value;
                    this.FirePropertyChanged("HasMessage");
                }
            }
        }
        

        ///<summary>User Message</summary>
        public object UserMessage {
            get { return this._userMessage; }
            set {
                if (this._userMessage != value) {
                    this._userMessage = value;
                    this.FirePropertyChanged("UserMessage");
                    this.HasMessage = (value != null);
                }
            }
        }
        

        ///<summary>Status</summary>
        public UserMessageTypes Status {
            get { return this._status; }
            set {
                if (this._status != value) {
                    this._status = value;
                    this.FirePropertyChanged("Status");
                }
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear() {
            this.Status = UserMessageTypes.Undefined;
            this.UserMessage = null;
        }

        /// <summary>
        /// Sets the message.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="message">The message.</param>
        public void SetMessage(UserMessageTypes messageType, object message) {
            this.Status = messageType;
            this.UserMessage= message;
        }
    }

    /// <summary>
    /// Base class for the workspace (e.g. form) View Model (MVVM pattern).
    /// </summary>
    public abstract class WorkspaceViewModel : ViewModelBase {
        ///<summary>Message Processor</summary>
        private IMessagePresenter _status;
        ///<summary>Service Locator</summary>
        private IServiceLocator _locator;

        ///<summary>Service Locator</summary>
        public IServiceLocator Locator {
            get {
                if (this._locator == null)
                    this._locator = AppContext.Current.GetServiceViaLocator<IServiceLocator>();
                return this._locator; 
            }
            set { this._locator = value; }
        }
        

        ///<summary>Message Processor</summary>
        public IMessagePresenter Status {
            get { return this._status; }
            set {
                if (this._status != value) {
                    this._status = value;
                    this.FirePropertyChanged("Status");
                }
            }
        }

        /// <summary>Executes the and report.</summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public virtual bool ExecuteAndReport(Action action) {
            try {
                action();
                return true;
            }
            catch (Exception x) {
                this.ReportError(x);
                return false;
            }
        }

        /// <summary>
        /// Reports the error. Default implementation distinguish <see cref="BusinessException"/> and technical faults.
        /// Technical faults are logged to the default log and user is presented with the stadard message and log refNo.
        /// </summary>
        /// <param name="x">The x.</param>
        public virtual void ReportError(Exception x) {
            AggregateException aex = x as AggregateException;
            if (aex != null)
                x = aex.GetFirstAggregate();
             BizException bbx; // Mess with Ops.Framework artifacts
            if (null != (bbx = x.TryConvertTo<BizException>())) {
                this.RunOnUIThread(() => this.Status.SetError(bbx.Message));
            }
            else {
                string refNo = AppContext.Current.LogTechError(string.Format("Technical error {0}: {1}", x.GetType().Name, x.Message), x);
                this.RunOnUIThread(() => this.Status.SetError(string.Format("Technical error. Please contact System Administrator. Log reference #{0}\r\n{1}", refNo, x.ToShortMsg())));
            }
        }


        /// <summary>
        /// Checks if he specified task has been completed (return true) or failed.
        /// If task has been failed then error is reported and return false;
        /// </summary>
        /// <param name="tsk">The TSK.</param>
        /// <returns></returns>
        public virtual bool CheckSuccess(Task tsk) {
            if (tsk.IsFaulted) {
                this.ReportError(tsk.Exception);
                return false;
            }
            else if (tsk.IsCanceled) {
                this.RunOnUIThread(() => this.Status.SetError("Operation has been canceled"));
                return false;
            }
            return true;
        }

        #region RequestClose [event]

        /// <summary>
        /// Raised when this workspace should be removed from the UI.
        /// </summary>
        public event EventHandler RequestClose;

        /// <summary>
        /// Initiate attempt to close the workspace.
        /// Normally workspace cannot close itself; it has to be done by the workspace manager.
        /// </summary>
        protected virtual void OnRequestClose() {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Find Command
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public abstract ICommand FindCommand(Func<ICommand, bool> filter);

        #endregion // RequestClose [event]

    }

    /// <summary>
    /// Base view interface
    /// </summary>
    public interface IBaseView {
        /// <summary>
        /// Sets the view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        void SetViewModel(ViewModelBase viewModel);
    }

    /// <summary>
    /// View Model for the container of workspaces
    /// </summary>
    public class WorkspaceContainerViewModel : WorkspaceViewModel {
        #region Private Members
        ///<summary>Workspaces</summary>
        private ObservableCollection<WorkspaceViewModel> _workspaces;
        ///<summary>Active Workspace</summary>
        private WorkspaceViewModel _activeWorkspace;
        ///<summary>Active workspace index</summary>
        private int _activeWorkspaceIndex= -1;

        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceContainerViewModel"/> class.
        /// </summary>
        public WorkspaceContainerViewModel() {
            InitWorkspaces();
        }
        #endregion

        #region Properties
        ///<summary>Workspaces</summary>
        public ObservableCollection<WorkspaceViewModel> Workspaces {
            get { return this._workspaces; }
        }

        ///<summary>Active Workspace</summary>
        public WorkspaceViewModel ActiveWorkspace {
            get { return this._activeWorkspace; }
            protected set {
                if (!object.ReferenceEquals(this._activeWorkspace, value)) {
                    LogChange("Changing 1", this.ActiveWorkspace, value);
                    this._activeWorkspace = value;
                    FirePropertyChanged("ActiveWorkspace");
                    LogChange("Changed 1", this.ActiveWorkspace, value);
                    this.ActiveWorkspaceIndex = this.Workspaces.IndexOf(value);
                }
            }
        }

        ///<summary>Active workspace index</summary>
        public int ActiveWorkspaceIndex {
            get { return this._activeWorkspaceIndex; }
            set {
                if (this._activeWorkspaceIndex != value) {
                    Log("Changing acive ix [{0}->{1}]", this._activeWorkspaceIndex, value);
                    this._activeWorkspaceIndex = value;
                    this.FirePropertyChanged("ActiveWorkspaceIndex");
                    Log("Changed acive ix [{0}->{1}]", this._activeWorkspaceIndex, value);
                    var currWs = (value < 0) ? null : this._workspaces[value];
                    if (currWs != this._activeWorkspace) {
                        LogChange("Changing 2", this.ActiveWorkspace, currWs);
                        this._activeWorkspace = currWs;
                        FirePropertyChanged("ActiveWorkspace");
                        LogChange("Changed 2", this.ActiveWorkspace, currWs);
                    }
                }
            }
        }

        #endregion

        #region Operations
        /// <summary>
        /// Inits the workspaces.
        /// </summary>
        void InitWorkspaces() {
            this._workspaces = new ObservableCollection<WorkspaceViewModel>();
            this._workspaces.CollectionChanged += new NotifyCollectionChangedEventHandler(_workspaces_CollectionChanged);
        }

        void Log(string fmt, params object[] args) {
            System.Diagnostics.Debug.WriteLine(fmt, args);
        }

        void LogChange(string opTxt, WorkspaceViewModel wsOld, WorkspaceViewModel wsNew) {
            Log("{4} active ws from [{0}:{1}] to [{2}:{3}]", wsOld, (wsOld == null) ? "null" : wsOld.Title, wsNew, (wsNew == null) ? "null" : wsNew.Title, opTxt);
        }

        /// <summary>
        /// Handles the CollectionChanged event of the _workspaces control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        void _workspaces_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.NewItems)
                    workspace.RequestClose += Workspace_RequestClose;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.OldItems)
                    workspace.RequestClose -= Workspace_RequestClose;
        }

        /// <summary>
        /// Called when workspace is being closed. This method is caled before workspace
        /// is removed from container and disposed.
        /// </summary>
        /// <param name="workspaceToClose">The workspace to close.</param>
        protected virtual void OnWorkspaceClose(WorkspaceViewModel workspaceToClose) {
        }

        /// <summary>
        /// Handles the RequestClose event of the _workspace control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Workspace_RequestClose(object sender, EventArgs e) {
            WorkspaceViewModel workspace = sender as WorkspaceViewModel;
            //int ix = this.Workspaces.IndexOf(workspace);
            OnWorkspaceClose(workspace);
            this.Workspaces.Remove(workspace); //.Remove(workspace);
            workspace.Dispose();
        }

        /// <summary>
        /// Adds the workspace and makes it Active Workspace.
        /// </summary>
        /// <param name="vmodel">The vmodel.</param>
        public void AddWorkspace(WorkspaceViewModel vmodel) {
            if (this._workspaces != null) {
                this._workspaces.Add(vmodel);
                this.ActiveWorkspace = vmodel;
                this.ActiveWorkspaceIndex = this.Workspaces.IndexOf(vmodel);
            }
        }


        /// <summary>
        /// Ensures that workspace exists in the container and make it active. Must be on UI thread.
        /// </summary>
        /// <param name="matchCheck">The match check. This predicate is used to detect workspace in 
        /// the container if it is already there. 
        /// Use this feature to ensure that there is only one workspace in the container with specific features.
        /// Use <c>null</c> to skip this step.</param>
        /// <param name="factoryMethod">The factory method to create workspace in order to add it into the container.
        /// </param>
        public WorkspaceViewModel EnsureWorkspace(Func<WorkspaceViewModel, bool> matchCheck, Func<WorkspaceViewModel> factoryMethod) {
            WorkspaceViewModel workspace = null;
            if (matchCheck != null)
                workspace = this.Workspaces.FirstOrDefault(matchCheck);
            if (workspace == null) {
                workspace = factoryMethod();
                this.Workspaces.Add(workspace);
            }
            int ix = this.Workspaces.IndexOf(workspace);
            this.ActiveWorkspace = workspace;
            this.ActiveWorkspaceIndex = ix; // #2
            return workspace;
        }


        /// <summary>
        /// Ensures that the workspace with the specified type exists in the container. Must be on UI thread.
        /// </summary>
        /// <typeparam name="Tworkspace">The type of the workspace.</typeparam>
        /// <returns></returns>
        public WorkspaceViewModel EnsureSingleWorkspace<Tworkspace>() where Tworkspace : WorkspaceViewModel, new() {
            Type tp = typeof(Tworkspace);
            return this.EnsureWorkspace((ws) => tp.IsInstanceOfType(ws), () => new Tworkspace());
        }


        /// <summary>
        /// Ensures the single workspace. Must be on UI thread.
        /// </summary>
        /// <typeparam name="Tworkspace">The type of the workspace.</typeparam>
        /// <param name="matchCheck">The match check. This predicate is used to detect workspace in 
        /// the container if it is already there. 
        /// Use this feature to ensure that there is only one workspace in the container with specific features.
        /// </param>
        /// <returns></returns>
        public Tworkspace EnsureSingleWorkspace<Tworkspace>(Func<Tworkspace, bool> matchCheck) where Tworkspace : WorkspaceViewModel, new() {
            Type tp = typeof(Tworkspace);
            return (Tworkspace)this.EnsureWorkspace((ws) => tp.IsInstanceOfType(ws) && matchCheck((Tworkspace)ws), () => new Tworkspace());
        }

        /// <summary>
        /// Ensures the single workspace. Must be on UI thread.
        /// </summary>
        /// <typeparam name="Tworkspace">The type of the workspace.</typeparam>
        /// <param name="matchCheck">The match check. This predicate is used to detect workspace in
        /// the container if it is already there.
        /// Use this feature to ensure that there is only one workspace in the container with specific features.</param>
        /// <param name="factoryMethod">The factory method to create new workspace.</param>
        /// <returns></returns>
        public Tworkspace EnsureSingleWorkspace<Tworkspace>(Func<Tworkspace, bool> matchCheck, Func<Tworkspace> factoryMethod) where Tworkspace : WorkspaceViewModel {
            Type tp = typeof(Tworkspace);
            return (Tworkspace)this.EnsureWorkspace((ws) => tp.IsInstanceOfType(ws) && matchCheck((Tworkspace)ws), () => factoryMethod());
        }

        /// <summary>
        /// Ensures the single workspace. May be on any thread.
        /// </summary>
        /// <typeparam name="Tworkspace">The type of the workspace.</typeparam>
        /// <param name="matchCheck">The match check. This predicate is used to detect workspace in
        /// the container if it is already there.
        /// Use this feature to ensure that there is only one workspace in the container with specific features.</param>
        /// <param name="factoryMethod">The factory method to create new workspace.</param>
        /// <param name="onComplete">The delegate to be executed on complete. Can be null.</param>
        public void EnsureSingleWorkspaceA<Tworkspace>(Func<Tworkspace, bool> matchCheck, Func<Tworkspace> factoryMethod, Action<Tworkspace> onComplete = null) where Tworkspace : WorkspaceViewModel {
            Type tp = typeof(Tworkspace);
            this.RunOnUIThread(() => {
                Tworkspace rz = (Tworkspace)this.EnsureWorkspace((ws) => tp.IsInstanceOfType(ws) && matchCheck((Tworkspace)ws), () => factoryMethod());
                if (onComplete != null)
                    onComplete(rz);
            });
        }

        /// <summary>
        /// Activates the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public void Activate(WorkspaceViewModel viewModel) {
            if (!this.Workspaces.Contains(viewModel))
                throw new ArgumentException("Workcpace cannot be activated if it is not inside the Workspace Container", "viewModel");
            this.ActiveWorkspace = viewModel;
        }

        /// <summary>
        /// Finds the command.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public override ICommand FindCommand(Func<ICommand, bool> filter) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when container is disposed.
        /// </summary>
        protected override void OnDispose() {
            base.OnDispose();
            var workSpaces = this.Workspaces;
            this.Workspaces.Clear();
            if (workSpaces != null)
                workSpaces.Apply((ws) => ws.Dispose());
        }
        #endregion
    }

    /// <summary>
    /// View Model for command
    /// </summary>
    public class CommandViewModel<Toperation> : ViewModelBase {
        #region Private Members
        ///<summary>Operation Id</summary>
        private Toperation _id;
        ///<summary>Command Name to be shown in menu or button</summary>
        private string _name;
        ///<summary>Command description to be used as a tooltip.</summary>
        private string _description;
        ///<summary>Command instance</summary>
        private ICommand _command;
        ///<summary>Visual cue</summary>
        private string _visualCue;

        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandViewModel&lt;Toperation&gt;"/> class.
        /// </summary>
        /// <param name="operationId">The operation id.</param>
        /// <param name="command">The command.</param>
        public CommandViewModel(Toperation operationId, ICommand command) {
            this._id = operationId;
            this._command = command;
//            this.LoadOperationDescription();
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="CommandViewModel&lt;Toperation&gt;"/> class.
        ///// </summary>
        ///// <param name="command">The command.</param>
        //public CommandViewModel(RelayCommand<Toperation> command) {
        //    this._id = command.OperationId;
        //    this._command = command;
        //    this.LoadOperationDescription();
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandViewModel&lt;Toperation&gt;"/> class.
        /// </summary>
        /// <param name="operationId">The operation id.</param>
        /// <param name="command">The command.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        public CommandViewModel(Toperation operationId, ICommand command, string name, string description) {
            this._id = operationId;
            this._command = command;
            this._name = name;
            this._description = description;
        }

        #endregion

        #region Properties

        ///<summary>Operation Id</summary>
        public Toperation Id {
            get { return this._id; }
            set {
                if (!object.Equals(this._id, value)) {
                    this._id = value;
                    this.FirePropertyChanged("Id");
                }
            }
        }

        ///<summary>Command Name to be shown in menu or button</summary>
        public string Name {
            get { return this._name??this.Id.ToString(); }
            set {
                if (this._name != value) {
                    this._name = value;
                    this.FirePropertyChanged("Name");
                }
            }
        }

        ///<summary>Command description to be used as a tooltip.</summary>
        public string Description {
            get { return this._description ?? this.Id.ToString(); }
            set {
                if (this._description != value) {
                    this._description = value;
                    this.FirePropertyChanged("Description");
                }
            }
        }

        ///<summary>Visual cue</summary>
        public string VisualCue {
            get { return this._visualCue; }
            set {
                if (this._visualCue != value) {
                    this._visualCue = value;
                    this.FirePropertyChanged("VisualCue");
                }
            }
        }
        

        ///<summary>Command instance</summary>
        public ICommand Command {
            get { return this._command; }
            private set { this._command = value; }
        }

        #endregion
    }

    /// <summary>
    /// This factory create initialized View for the specified View Model
    /// </summary>
    public class ViewFactory {
        ///<summary>View Model to View mapping</summary>
        private Dictionary<Type, Func<IBaseView>> _map = new Dictionary<Type, Func<IBaseView>>();

        ///<summary>View Model to View mapping</summary>
        public Dictionary<Type, Func<IBaseView>> Map {
            get { return this._map; }
        }


        /// <summary>
        /// Creates the view.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <returns></returns>
        public IBaseView CreateView(ViewModelBase viewModel) {
            IBaseView result = null;
            if (viewModel != null) {
                Func<IBaseView> factoryMethod;
                if (this.Map.TryGetValue(viewModel.GetType(), out factoryMethod)) {
                    result = factoryMethod();
                    result.SetViewModel(viewModel);
                }
            }
            return result;
        }


        /// <summary>
        /// Register the standart view creator via <c>new</c> for the specified view model.
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <typeparam name="TView">The type of the view.</typeparam>
        public void Register<TViewModel, TView>()
            where TViewModel : ViewModelBase
            where TView : IBaseView, new() {
            Map[typeof(TViewModel)] = () => new TView();
        }

        /// <summary>
        /// Registers the singleton. View will be created after the first request and will remain
        /// afterwards.
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <typeparam name="TView">The type of the view.</typeparam>
        public void RegisterSingleton<TViewModel, TView>()
            where TViewModel : ViewModelBase
            where TView : IBaseView, new() {
            Singleton tmp = new Singleton(() => new TView());
            Map[typeof(TViewModel)] = tmp.CreateView;
        }

        /// <summary>
        /// Registers the singleton instance.
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <param name="view">The view.</param>
        public void RegisterSingletonInstance<TViewModel>(IBaseView view) where TViewModel : ViewModelBase {
            Singleton tmp = new Singleton(view);
            Map[typeof(TViewModel)] = tmp.CreateView;
        }

        class Singleton {
            private IBaseView _view;
            private Func<IBaseView> _factoryMethod;

            public Singleton(Func<IBaseView> factoryMethod) {
                this._factoryMethod = factoryMethod;
            }

            public Singleton(IBaseView view) {
                this._view = view;
            }

            /// <summary>
            /// Creates the view.
            /// </summary>
            /// <returns></returns>
            public IBaseView CreateView() {
                if (this._view == null) {
                    this._view = _factoryMethod();
                }
                return this._view;
            }
        }
    }

    /// <summary>
    /// Information required by seelction View Model to conduct selection
    /// </summary>
    /// <typeparam name="Tresult"></typeparam>
    public class SelectorVMOptions<Tresult> {
        /// <summary>
        /// Gets or sets the prompt. Prompt will be shown to user during the selection process.
        /// </summary>
        /// <value>The prompt.</value>
        public object Prompt { get; set; }
        /// <summary>
        /// Gets or sets the criteria restrictions. Information regarding what search criteria must be fixed
        /// in order to conduct search. Can be <c>null</c> if there are no restrictions.
        /// </summary>
        /// <value>The criteria restrictions.</value>
        public object CriteriaRestrictions { get; set; }
        /// <summary>
        /// Delegate to be called when user selects the item. Can not be null.
        /// The delegate can throw a Business Exception if seelction is not acceptable.
        /// </summary>
        /// <value>Delegate to be called when user selects the item.</value>
        public Action<Tresult> OnSelected { get; set; }
        /// <summary>
        /// Delegate to be called when user exits without selecting anything.
        /// </summary>
        /// <value>Action to be performed on exit without selection.</value>
        public Action OnCancel { get; set; }
    }

    /// <summary>
    /// View Model contract to behave as selector.
    /// Typical example is Search View Model used in selector mode to pick up record.
    /// </summary>
    public interface ISelectorVModel<Tresult> {
        /// <summary>
        /// Sets the selection mode.
        /// </summary>
        /// <param name="options">The options.</param>
        void SetSelectionMode(SelectorVMOptions<Tresult> options);
    }

    /// <summary>
    /// Wrapper class that just adds Marker to the wrapped class
    /// </summary>
    /// <typeparam name="TMarker">The type of the marker.</typeparam>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public class MarkerWrap<TMarker, TData> : NotifyPropertyChangedBase, INotifyPropertyChanged {
        ///<summary>Marker</summary>
        private TMarker _marker;
        ///<summary>Data</summary>
        private TData _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkerWrap{TData}" /> class.
        /// </summary>
        public MarkerWrap() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkerWrap{TData}" /> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public MarkerWrap(TData data) {
            this._data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkerWrap{TData}" /> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="marker">The marker.</param>
        public MarkerWrap(TData data, TMarker marker) {
            this._data = data;
            this._marker = marker;
        }

        ///<summary>Data</summary>
        public TData Data {
            get { return this._data; }
            set {
                if (!object.Equals(this._data, value)) {
                    this._data = value;
                    this.FirePropertyChanged("Data");
                }
            }
        }


        ///<summary>Marker</summary>
        public TMarker Marker {
            get { return this._marker; }
            set {
                if (!object.Equals(this._marker, value)) {
                    this._marker = value;
                    this.FirePropertyChanged("Marker");
                }
            }
        }

    }


    /// <summary>
    /// Extensions methods for MVVM support
    /// </summary>
    public static class MvvmExtensions {
        /// <summary>
        /// Sets the error message to be visible to user.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="x">The exception.</param>
        public static void SetError(this IMessagePresenter presenter, object message, Exception x = null) {
            if (presenter != null) {
                if (x != null) {
                    BizException bx = x as BizException;
                    if (bx != null) {
                        message = string.Format("{0}: {1}", message ?? "Operation failed", bx.Message);
                    }
                    else {
                        message = string.Format("{0}\r\n{1}", message, x.GetDetails());
                    }
                }
                presenter.SetMessage(UserMessageTypes.Error, message);
            }
        }

        /// <summary>
        /// Sets the neutral message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void SetNeutral(this IMessagePresenter presenter, object message) {
            if (presenter != null) {
                presenter.SetMessage(UserMessageTypes.Neutral, message);
            }
        }

        /// <summary>
        /// Sets the positive message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void SetPositive(this IMessagePresenter presenter, object message) {
            if (presenter != null) {
                presenter.SetMessage(UserMessageTypes.Positive, message);
            }
        }

    }

#region Converters
    /// <summary>
    /// Converts boolean value to <see cref="Visibility"/> value.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter {

        #region IValueConverter Members

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((Visibility)value) == Visibility.Visible;
        }

        #endregion
    }

    /// <summary>
    /// Converts is Null value to <see cref="Visibility"/> value.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter {

        #region IValueConverter Members

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (value==null) ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }

        #endregion
    }


    /// <summary>
    /// Converts boolean value to opposite value.
    /// </summary>
    public class BoolNegateConverter : IValueConverter {

        #region IValueConverter Members

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return !(bool)value;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return !(bool)value;
        }

        #endregion
    }


    /// <summary>
    /// Multiplies two numbers
    /// </summary>
    public class MultiplicationConverter : IValueConverter {
        ///<summary>Min value</summary>
        private double? _minValue;
        ///<summary>Max value</summary>
        private double? _maxValue;

        ///<summary>Max value</summary>
        public double? MaxValue {
            get { return this._maxValue; }
            set { this._maxValue = value; }
        }

        ///<summary>Min value</summary>
        public double? MinValue {
            get { return this._minValue; }
            set { this._minValue = value; }
        }

        double? ToNDouble(object src) {
            double t;
            double? val = null;
            if (src != null) {
                if (Double.TryParse(src.ToString(), out t)) val = t;
            }
            return val;
        }

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            double t;
            double? val=ToNDouble(value);
            double? prm = ToNDouble(parameter);
            if (val.HasValue && prm.HasValue) {
                t = val.Value * prm.Value;
                if ((this.MinValue.HasValue) && (t < this.MinValue.Value))
                    t = this.MinValue.Value;
                if ((this.MaxValue.HasValue) && (t < this.MaxValue.Value))
                    t = this.MaxValue.Value;
                return t;
            }
            return null;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Mapping converter entry.
    /// Instance of this class defines map from one object to another.
    /// </summary>
    public class MappingConverterItem {
        ///<summary>Source</summary>
        private object _source;
        ///<summary>Target</summary>
        private object _target;

        ///<summary>Target</summary>
        public object Target {
            get { return this._target; }
            set { this._target = value; }
        }

        ///<summary>Source</summary>
        public object Source {
            get { return this._source; }
            set { this._source = value; }
        }

    }

    /// <summary>
    /// Converts values via map that can be configured in XAML.
    /// </summary>
    public class MappingConverter : IValueConverter {
        /////<summary>Mapping</summary>
        private Dictionary<object, object> _map;
        ///<summary>Entry List</summary>
        private List<MappingConverterItem> _items = new List<MappingConverterItem>();
        ///<summary>Use string key</summary>
        private bool _useStringKey;
        ///<summary>Default result value</summary>
        private object _defaultResult;

        ///<summary>Use string key</summary>
        public bool UseStringKey {
            get { return this._useStringKey; }
            set {
                this._useStringKey = value;
                this._map = null;
            }
        }

        ///<summary>Default result value</summary>
        public object DefaultResult {
            get { return this._defaultResult; }
            set { this._defaultResult = value; }
        }


        ///<summary>Entry List</summary>
        public List<MappingConverterItem> Items {
            get { return this._items; }
            set {
                this._items = value;
                this._map = null;
            }
        }

        object ToStringKey(object src) {
            return (src == null) ? string.Empty : src.ToString();
        }

        ///<summary>Mapping</summary>
        public Dictionary<object, object> Map {
            get {
                if (this._map == null) {
                    this._map = new Dictionary<object, object>();
                    Func<object, object> toKey = ToStringKey;
                    if (!this.UseStringKey)
                        toKey = (p) => { return p; };
                    if (this.Items!=null) {
                        foreach (var item in this.Items) {
                            this._map[toKey(item.Source)] = item.Target;
                        }
                    }
                }
                return this._map;
            }
            //set { this._map = value; }
        }


        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            object result;
            if (this.UseStringKey) {
                value = ToStringKey(value);
            }
            if (this.Map.TryGetValue(value, out result))
                return result;
            return this.DefaultResult;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// String to DateTime? converter
    /// </summary>
    public class TimestampNStringConverter : IValueConverter {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            DateTime? src = value as DateTime?;
            if (src.HasValue) return src.Value.ToString("yyyy-MM-dd HH:mm:ss");
            return string.Empty;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return null;
            string s = value.ToString();
            if (string.IsNullOrWhiteSpace(s)) return null;
            return (DateTime?)DateTime.Parse(s, culture);
        }
    }

    /// <summary>
    /// Converts <c>int</c> to <c>int?</c> and back.
    /// Automatically adjust to both directions by actual type of the incoming value to convert.
    /// Useful to set search criteria nullable reference Id.
    /// </summary>
    public class IntIntNConverter : IValueConverter {
        /// <summary>Converts a value.</summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            int src;
            int? rz;
            if (value == null) return int.MinValue;
            if (value.GetType() == typeof(int?)) { // int?->int
                rz = (int?)value;
                return rz.Value; // Null check alredy done above
            }
            else { // int->int?
                src = (int)value;
                if (src == int.MinValue)
                    return null;
                else
                    return src;
            }
        }

        /// <summary>Converts a value.</summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Convert(value, targetType, parameter, culture);
        }

    }

    /// <summary>
    /// Special converter for nullable values (including strings).
    /// </summary>
    public class NullableConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((value == null) || string.IsNullOrEmpty(value.ToString()))
                return null;
            return value;
        }
    }

    public class FractionConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

    }
    #endregion
}

using System;
using System.Windows.Input;
using System.Diagnostics;
using System.ComponentModel;
using Ops.NetCoe.LightFrame;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace XC.MediaRat {

    /// <summary>
    /// Ping UI Operations
    /// </summary>
    public enum UIOperations {
        Undefined= 0,
        Clear=1,
        AddSource=2,
        ShowLogs=3,
        Clone=4,
        Delete=5,
        GetXml=6,
        ReadXml=7,
        Select=8,
        CommandGroup=9,
        Apply=10,
        Save=11,
        Open=12,
        Restore= 14,
        SaveAs=15,
        Cancel=16,
        Copy=17,
        Search,
        ChangeActive,
        FilterAdd,
        FilterDelete,
        AddItem,
        DeleteItem,
        EditItem,
        AddSourceFilter,
        AddSourceFilterSubTree,
        DeleteSourceFilter,
        AddRating,
        DeleteRating,
        AddCategory,
        DeleteCategory,
        Popout,
        ReleaseFilter,
        FindNext,
        MoveNext,
        MovePrevious,
        Rename,
        Move,
        Unselect,
        Inverse,
        Export2MovieMaker,
        ExportMediaSheet,
        PopulateMediaProps,
        GetPropElements,
        ApplyPropElements,
        CreateTrack,
        CreateTrackGroup,
        AdjustOrderWeights,
        OrderWeightSet,
        SlideShow,
        //CreateDeploymentTask,
        //CreateTaskGroup,
        //CreateWebSiteTask,
        //CreateWebAppTask,
        //CreateAppPoolTask,
        //CreateHostsTask,
        //ImportWebSite,
        //ImportWebApp,
        //ImportAppPool,
        //ImportHosts,
        //ImportProjectXml,
        //ExportProjectXml,
        //AddBinding,
        //AppPoolSelect,
        //WebSiteSelect,
        //GetWebSiteDeployTaskXml,
        //Deploy,
        //AddDeploymentTaskTab,
        //AddDeploymentProjectTab,
        //AddParam,
        //DeleteParam,
        Exit
    }

    /// <summary>
    /// Class that binds <see cref="ICommand"/> interface with operation description and MVVM pattern.
    /// </summary>
    public class RelayCommand : ICommand, INotifyPropertyChanged {
        #region Private Members
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;
        private bool _canExecuteFlag;
        ///<summary>Operation Id</summary>
        private UIOperations _operationId;
        ///<summary>Context of the current asynchronous operation. Used for Abort operations.</summary>
        private WinAsyncCallContext _asyncContext;
        ///<summary>Is Executing</summary>
        private bool _isExecuting;


        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The delegate to execute.</param>
        public RelayCommand(UIOperations operationId, Action<object> execute)
            : this(operationId, execute, null) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The delegate to execute.</param>
        /// <param name="canExecute">The delegate to verify if command can be executed.</param>
        public RelayCommand(UIOperations operationId, Action<object> execute, Predicate<object> canExecute) {
            this._operationId = operationId;
            if (execute == null)
                throw new ArgumentNullException("execute", "Delegate 'execute' not specified in the constructor for operationId "+operationId.ToString());
            _execute = execute;
            _canExecute = canExecute;
        }

 
        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand&lt;PingUIOperations&gt;"/> class.
        /// </summary>
        /// <param name="operationId">The operation id.</param>
        /// <param name="canExecute">The can execute method. use <c>null</c> to skip the check.</param>
        /// <param name="beginExecute">The begin execute method. Method that should be called when execution 
        /// completed should be specified in the <see cref="IAsyncCallContext.EndCall"/> inside the <c>beginExecute</c>.</param>
        public RelayCommand(UIOperations operationId, BeginAsyncOperationAction beginExecute, Predicate<object> canExecute) {
            this._operationId = operationId;
            this._canExecute = canExecute;
            this._execute = (prm) => {
                this.IsExecuting = null !=
                beginExecute(this._asyncContext = new WinAsyncCallContext(this.OperationId, prm), EndAsyncOperation); 
            };
        }


        ///// <summary>
        ///// Initializes a new instance of the <see cref="RelayCommand&lt;PingUIOperations&gt;"/> class.
        ///// </summary>
        ///// <param name="operationId">The operation id.</param>
        ///// <param name="canExecute">The can execute method. use <c>null</c> to skip the check.</param>
        ///// <param name="beginExecute">The begin execute method.</param>
        ///// <param name="onExecuteCompleted">The method to be called when execution is completed.</param>
        //public RelayCommand(PingUIOperations operationId, Predicate<object> canExecute, BeginAsyncOperationAction beginExecute, AsyncCallback onExecuteCompleted) {
        //    this._operationId = operationId;
        //    this._canExecute = canExecute;
        //    this._execute = (prm) => beginExecute(new SilverlightAsyncCallContext(this.OperationId, prm), EndAsyncOperation);
        //}

        #endregion

        #region Properties
        ///<summary>Operation Id</summary>
        public UIOperations OperationId {
            get { return this._operationId; }
            protected set { this._operationId = value; }
        }

        ///<summary>Is Executing</summary>
        public bool IsExecuting {
            get { return this._isExecuting; }
            protected set {
                if (this._isExecuting != value) {
                    this._isExecuting = value;
                    this.FirePropertyChanged("IsExecuting");
                }
            }
        }

        #endregion

        #region Operations


        /// <summary>
        /// Fires the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void FirePropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) {
                if (this._asyncContext != null) {
                    if (!this._asyncContext.Dispatcher.CheckAccess()) {
                        this._asyncContext.Dispatcher.BeginInvoke(handler, this, new PropertyChangedEventArgs(propertyName));
                        return;
                    }
                }
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        void EndAsyncOperation(IAsyncResult asyncResult) {
            this._asyncContext = null; // Clear the cached reference
            WinAsyncCallContext asyncCallContext = ((WinAsyncCallContext)(asyncResult.AsyncState));
            asyncCallContext.RestoreThreadContext();
            if (asyncCallContext.Dispatcher.CheckAccess()) {
                this.IsExecuting = false;
                asyncCallContext.EndCall(asyncResult);
            }
            else {
                asyncCallContext.Dispatcher.BeginInvoke(new Action(() => {
                    this.IsExecuting = false;
                    asyncCallContext.EndCall(asyncResult);
                }));
            }
        }

        /// <summary>
        /// Resets the command status using the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void Reset(object parameter=null) {
            bool tmp= this.CanExecute(parameter);
            if (tmp!=this._canExecuteFlag) {
                this._canExecuteFlag= tmp;
                if (CanExecuteChanged != null)
                    CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Aborts the execution.
        /// </summary>
        public virtual bool Abort() {
            WinAsyncCallContext asyncCtx = this._asyncContext;
            if (this.IsExecuting && (asyncCtx != null) && this._asyncContext.AbortCall!=null) {
                return asyncCtx.AbortCall(asyncCtx);
            }
            return false;
        }

        /// <summary>
        /// Executes if <see cref="CanExecute"/> is true.
        /// </summary>
        /// <param name="prm">The parameter.</param>
        public virtual void ExecuteIfCan(object prm) {
            if (this.CanExecute(prm))
                this.Execute(prm);
        }


        /// <summary>
        /// Executes if <see cref="CanExecute"/> is true using <c>null</c> as parameter.
        /// </summary>
        public virtual void ExecuteIfCan() {
            ExecuteIfCan(null);
        }

        /// <summary>
        /// Dumps this instance.
        /// </summary>
        public virtual void Dump() {
            if (this.CanExecuteChanged == null) {
                System.Diagnostics.Debug.WriteLine("|-> invocation list: null");
            }
            else {
                Delegate[] delegeates= this.CanExecuteChanged.GetInvocationList();
                System.Diagnostics.Debug.WriteLine("|-> invocation list: "+delegeates.Length.ToString());
                foreach (var d in delegeates) {
                    System.Diagnostics.Debug.WriteLine("|->> " + d.Method.DeclaringType.FullName + "." + d.Method.Name);
                }
            }
        }

        #endregion


        #region ICommand Members

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter) {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        ///// <summary>
        ///// Occurs when changes occur that affect whether the command should execute.
        ///// </summary>
        //public event EventHandler CanExecuteChanged {
        //    add { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}


        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter) {
            _execute(parameter);
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

    }

    /// <summary>
    /// VModel for command
    /// </summary>
    public class CommandVModel : CommandViewModel<UIOperations> {
        const string vCue = "CommandVModel";
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandVModel"/> class.
        /// </summary>
        public CommandVModel(RelayCommand cmd)
            : base(cmd.OperationId, cmd) {
            this.VisualCue = vCue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandVModel"/> class.
        /// </summary>
        /// <param name="cmd">The CMD.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        public CommandVModel(RelayCommand cmd, string name, string description)
            : this(cmd) {
            this.Name = name;
            this.Description = description;
            this.VisualCue = vCue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandVModel"/> class.
        /// </summary>
        /// <param name="opId">The op id.</param>
        /// <param name="cmd">The CMD.</param>
        public CommandVModel(UIOperations opId, ICommand cmd) : base(opId, cmd) {
            this.VisualCue = vCue;
        }
    }


    /// <summary>
    /// View model for the group of commands
    /// </summary>
    public class CommandGroupVModel : CommandVModel, ICommandVModelSource {
        ///<summary>Command View Modles</summary>
        private ObservableCollection<CommandVModel> _commandVModels;

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="SdePortalCommandGroupViewModel"/> class.
        /// </summary>
        public CommandGroupVModel()
            : base(UIOperations.CommandGroup, null) {
                this.VisualCue = "CommandGroupVModel";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SdePortalCommandGroupViewModel"/> class.
        /// </summary>
        /// <param name="commandVModels">The command V models.</param>
        public CommandGroupVModel(params CommandVModel[] commandVModels)
            : this() {
            this._commandVModels = new ObservableCollection<CommandVModel>();
            foreach (var cvm in commandVModels) {
                this._commandVModels.Add(cvm);
            }
        }

        #endregion

        #region Properties

        ///<summary>Command View Modles</summary>
        public ObservableCollection<CommandVModel> CommandVModels {
            get { return this._commandVModels; }
            set {
                if (this._commandVModels != value) {
                    this._commandVModels = value;
                    this.FirePropertyChanged("CommandVModels");
                }
            }
        }
        #endregion

        #region Operations
        /// <summary>
        /// Enumerates the commands.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RelayCommand> EnumerateCommands() {
            var cvm = this.CommandVModels;
            if ((cvm!=null) && (cvm.Count>0)) {
                RelayCommand cmd;
                foreach (var vm in cvm) {
                    cmd = vm.Command as RelayCommand;
                    if (cmd != null)
                        yield return cmd;
                }
            }
        }

        #endregion
    }


    /// <summary>
    /// Contract for the provider of Command View Models
    /// </summary>
    public interface ICommandVModelSource {
        ///<summary>Command View Modles</summary>
        ObservableCollection<CommandVModel> CommandVModels { get; }
    }
}

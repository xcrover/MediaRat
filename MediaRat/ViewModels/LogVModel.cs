using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ops.NetCoe.LightFrame;
using System.Windows.Input;
using System.Collections.ObjectModel;


namespace XC.MediaRat {
    ///<summary>Log View Model</summary>
    public class LogVModel : WorkspaceViewModel, ILog {
        #region Business Data
        private object _lock = new Object();
        int _cnt = 1;
        ///<summary>Logs</summary>
        private ObservableCollection<CodeValuePair> _logs = new ObservableCollection<CodeValuePair>();
        private WinLog _wLog;

        ///<summary>Logs</summary>
        public ObservableCollection<CodeValuePair> Logs {
            get { return this._logs; }
        }

        #endregion

        #region Commands
        ///<summary>Command VModels</summary>
        private ObservableCollection<CommandVModel> _commandVModels;
        ///<summary>Exit Command</summary>
        private RelayCommand _exitCmd;
        ///<summary>Clear Log Command</summary>
        private RelayCommand _clearLogCmd;

        ///<summary>Clear Log Command</summary>
        public RelayCommand ClearLogCmd {
            get { return this._clearLogCmd; }
        }

        ///<summary>Command VModels</summary>
        public ObservableCollection<CommandVModel> CommandVModels {
            get { return this._commandVModels; }
            set {
                if (this._commandVModels != value) {
                    this._commandVModels = value;
                    this.FirePropertyChanged("CommandVModels");
                }
            }
        }

        ///<summary>Exit Command</summary>
        public RelayCommand ExitCommand {
            get { return this._exitCmd; }
        }

        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="LogVModel"/> class.
        /// </summary>
        public LogVModel()
            : this("Log") {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogVModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public LogVModel(string title) {
            this.Title = title;
            this._wLog = new WinLog("XC.MediaRat");
            //InitTests();
            this.InitCommands();
        }
        #endregion

        #region Command plumbing
        void DoExit(object prm) {
            this.OnRequestClose();
        }

        void DoClearLog(object prm) {
            this.Logs.Clear();
        }


        /// <summary>
        /// Enumerate all the available commands
        /// </summary>
        IEnumerable<ICommand> EnumerateCommands() {
            yield return this.ClearLogCmd;
            yield return this.ExitCommand;
        }

        /// <summary>
        /// Initialize commands infrastructure
        /// </summary>
        void InitCommands() {
            this._clearLogCmd = new RelayCommand(UIOperations.Clear, DoClearLog, (p) => true);
            this._exitCmd = new RelayCommand(UIOperations.Exit, DoExit, (p) => true);

            ObservableCollection<CommandVModel> cmdVms = new ObservableCollection<CommandVModel>();
            cmdVms.Add(new CommandVModel(ClearLogCmd) { Name = "Clear", Description = "Clear log" });
            //cmdVms.Add(new CommandVModel(ExitCommand));
            //cmdVms.Add(new CommandVModel(ClonetCmd) { Name = "Clone", Description = "Clone workspace" });
            CommandVModels = cmdVms;
        }

        /// <summary>
        /// Reset presentation attributes according to the current state
        /// </summary>
        void ResetViewState() {
            foreach (var cmd in this.EnumerateCommands()) cmd.CanExecute(null);
        }

        /// <summary>Called when <see cref="IsBusy"/> changed.</summary>
        /// <param name="newValue">New value of <see cref="IsBusy"/>.</param>
        protected override void OnIsBusyChanged(bool newValue) {
            base.OnIsBusyChanged(newValue);
            ResetViewState();
        }

        /// <summary>
        /// Find Command
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public override ICommand FindCommand(Func<ICommand, bool> filter) {
            return this.EnumerateCommands().FirstOrDefault(filter);
        }

        #endregion

        /// <summary>
        /// Logs the technical error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="x">The exception.</param>
        /// <returns>
        /// Log id or reference number if available
        /// </returns>
        public string LogTechError(string message, Exception x) {
            this._wLog.LogTechError(message, x);
            CodeValuePair itm = new CodeValuePair("Error", string.Format("{0}\r{1}", message, (x == null) ? string.Empty : x.GetDetails()));
            string id;
            lock (this._lock) {
                id = (this._cnt++).ToString();
                RunOnUIThread(() => this.Logs.Add(itm));
            }
            return id;
        }

        /// <summary>
        /// Logs the trace.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public string LogTrace(string message) {
            CodeValuePair itm = new CodeValuePair("Trace", message);
            string id;
            lock (this._lock) {
                id = (this._cnt++).ToString();
                RunOnUIThread(() => this.Logs.Add(itm));
            }
            return id;
        }

        /// <summary>
        /// Logs the warning.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public string LogWarning(string message) {
            this._wLog.LogTrace(message);
            CodeValuePair itm = new CodeValuePair("Warning", message);
            string id;
            lock (this._lock) {
                id = (this._cnt++).ToString();
                RunOnUIThread(() => this.Logs.Add(itm));
            }
            return id;
        }
    }
}

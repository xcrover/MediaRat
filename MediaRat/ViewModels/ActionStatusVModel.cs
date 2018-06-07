using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {

    ///<summary>View Model for Operation execution</summary>
    public class ActionStatusVModel : WorkspaceViewModel {
        ///<summary>Min boundary</summary>
        private int _minIndex;
        ///<summary>Max Index</summary>
        private int _maxIndex;
        ///<summary>Current Index</summary>
        private int _currentIndex;
        ///<summary>Error count</summary>
        private int _errorCount;
        ///<summary>Action to execute on Exit command</summary>
        private Action<ActionStatusVModel> _exitHitAction;

        ///<summary>Action to execute on Exit command</summary>
        public Action<ActionStatusVModel> ExitHitAction {
            get { return this._exitHitAction; }
            set {
                this._exitHitAction = value;
                this.ResetViewState();
            }
        }


        ///<summary>Error count</summary>
        public int ErrorCount {
            get { return this._errorCount; }
            set {
                if (this._errorCount != value) {
                    this._errorCount = value;
                    this.FirePropertyChanged("ErrorCount");
                }
            }
        }
        

        ///<summary>Current Index</summary>
        public int CurrentIndex {
            get { return this._currentIndex; }
            set {
                if (this._currentIndex != value) {
                    this._currentIndex = value;
                    this.FirePropertyChanged("CurrentIndex");
                }
            }
        }

        ///<summary>Max Index</summary>
        public int MaxIndex {
            get { return this._maxIndex; }
            set {
                if (this._maxIndex != value) {
                    this._maxIndex = value;
                    this.FirePropertyChanged("MaxIndex");
                }
            }
        }
        

        ///<summary>Min boundary</summary>
        public int MinIndex {
            get { return this._minIndex; }
            set {
                if (this._minIndex != value) {
                    this._minIndex = value;
                    this.FirePropertyChanged("MinIndex");
                }
            }
        }
        
        #region Commands
        ///<summary>Command VModels</summary>
        private ObservableCollection<CommandVModel> _commandVModels;
        ///<summary>Exit Command</summary>
        private RelayCommand _exitCmd;

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
        /// Initializes a new instance of the <see cref="ActionStatusVModel"/> class.
        /// </summary>
        public ActionStatusVModel()
            : this("ActionStatusVModel") {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionStatusVModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public ActionStatusVModel(string title) {
            this.Title = title;
            Init();
            this.InitCommands();
        }

        /// <summary>
        /// Initialize business content.
        /// This method can be called more than once
        /// </summary>
        void Init() {
            this.Status = new StatusVModel();
        }
        #endregion

        #region Command plumbing
        void DoExit(object prm) {
            if (this.ExitHitAction!=null) {
                this.ExitHitAction(this);
            }
        }

        /// <summary>
        /// Enumerate all the available commands
        /// </summary>
        IEnumerable<RelayCommand> EnumerateCommands() {
            yield return this.ExitCommand;
        }

        /// <summary>
        /// Initialize commands infrastructure
        /// </summary>
        void InitCommands() {
            this._exitCmd = new RelayCommand(UIOperations.Exit, DoExit, (p) => { return this.ExitHitAction != null; });

            ObservableCollection<CommandVModel> cmdVms = new ObservableCollection<CommandVModel>();
            cmdVms.Add(new CommandVModel(ExitCommand));
            //cmdVms.Add(new CommandVModel(ClonetCmd) { Name = "Clone", Description = "Clone workspace" });
            CommandVModels = cmdVms;
        }

        /// <summary>
        /// Reset presentation attributes according to the current state
        /// </summary>
        void ResetViewState() {
            foreach (var cmd in this.EnumerateCommands()) cmd.Reset(null);
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
        /// Sets the success.
        /// </summary>
        /// <param name="currentIndex">Index of the current.</param>
        /// <param name="message">The message.</param>
        public void SetSuccess(int currentIndex, string message = null) {
            this.RunOnUIThread(() => {
                this.CurrentIndex = currentIndex;
                if (!string.IsNullOrEmpty(message)) {
                    this.Status.SetPositive(message);
                }
            });
        }

        /// <summary>
        /// Sets the error.
        /// </summary>
        /// <param name="currentIndex">Index of the current.</param>
        /// <param name="message">The message.</param>
        public void SetError(int currentIndex, string message = null) {
            this.RunOnUIThread(() => {
                this.CurrentIndex = currentIndex;
                this.ErrorCount += 1;
                if (!string.IsNullOrEmpty(message)) {
                    this.Status.SetError(message);
                }
            });
        }

    }

    
}

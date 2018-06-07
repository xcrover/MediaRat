using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {

    ///<summary>View Model for Property Elements</summary>
    public class PropElementListVModel : WorkspaceViewModel {
        ///<summary>Property Eleemnts</summary>
        private ObservableCollection<PropElement> _entities;
        ///<summary>Action to execute on OK</summary>
        private Action<IEnumerable<PropElement>> _applicator;

        ///<summary>Action to execute on OK</summary>
        public Action<IEnumerable<PropElement>> Applicator {
            get { return this._applicator; }
            set { this._applicator = value; }
        }
        

        ///<summary>Property Eleemnts</summary>
        public ObservableCollection<PropElement> Entities {
            get { return this._entities; }
            set {
                if (this._entities != value) {
                    this._entities = value;
                    this.FirePropertyChanged("Entities");
                }
            }
        }
        
        #region Commands
        ///<summary>Command VModels</summary>
        private ObservableCollection<CommandVModel> _commandVModels;
        ///<summary>Exit Command</summary>
        private RelayCommand _exitCmd;
        ///<summary>OK Command</summary>
        private RelayCommand _okCmd;


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

        ///<summary>OK Command</summary>
        public RelayCommand OkCmd {
            get { return this._okCmd; }
        }


        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="PropElementListVModel"/> class.
        /// </summary>
        public PropElementListVModel()
            : this("Properties") {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropElementListVModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public PropElementListVModel(string title) {
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
            this.OnRequestClose();
        }

        ///<summary>Execute OK Command</summary>
        void DoOkCmd(object prm = null) {
            ExecuteAndReport(() => {
                this.Applicator(this.Entities);
                this.OnRequestClose();
            });
        }

        ///<summary>Check if OK Command can be executed</summary>
        bool CanOkCmd(object prm = null) {
            return this.Applicator!=null;
        }


        /// <summary>
        /// Enumerate all the available commands
        /// </summary>
        IEnumerable<RelayCommand> EnumerateCommands() {
            yield return this.OkCmd;
            yield return this.ExitCommand;
        }

        /// <summary>
        /// Initialize commands infrastructure
        /// </summary>
        void InitCommands() {
            this._okCmd = new RelayCommand(UIOperations.Select, DoOkCmd, CanOkCmd);
            this._exitCmd = new RelayCommand(UIOperations.Exit, DoExit, (p) => true);

            ObservableCollection<CommandVModel> cmdVms = new ObservableCollection<CommandVModel>();
            cmdVms.Add(new CommandVModel(OkCmd) { Name = "OK", Description = "Execute operation and close this dialog" });
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
    }

    
}

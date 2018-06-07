using Ops.NetCoe.LightFrame;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace XC.MediaRat {

    ///<summary>View Model for Text request</summary>
    public class TextRqtVModel : WorkspaceViewModel {
        ///<summary>Text</summary>
        private string _text;
        ///<summary>Label text</summary>
        private string _labelText;

        ///<summary>Label text</summary>
        public string LabelText {
            get { return this._labelText; }
            set {
                if (this._labelText != value) {
                    this._labelText = value;
                    this.FirePropertyChanged("LabelText");
                }
            }
        }

        public Action<string> Apply { get; set; }

        ///<summary>Text</summary>
        public string Text {
            get { return this._text; }
            set {
                if (this._text != value) {
                    this._text = value;
                    this.FirePropertyChanged("Text");
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

        ///<summary>OK Command</summary>
        public RelayCommand OkCmd {
            get { return this._okCmd; }
        }

        ///<summary>Exit Command</summary>
        public RelayCommand ExitCommand {
            get { return this._exitCmd; }
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

        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="TextRqtVModel"/> class.
        /// </summary>
        public TextRqtVModel() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextRqtVModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public TextRqtVModel(string title, string labelText, string initText, Action<string> apply) {
            this.Title = title;
            this.LabelText = labelText;
            this.Text = initText;
            this.Apply = apply;
            Init();
            this.InitCommands();
        }

        /// <summary>
        /// Initialize business content.
        /// This method can be called more than once
        /// </summary>
        void Init() {
            this.Status= new StatusVModel();
        }
        #endregion

        #region Command plumbing
        void DoExit(object prm) {
            this.OnRequestClose();
        }

        ///<summary>Execute OK Command</summary>
        void DoOkCmd(object prm = null) {
            if (this.ExecuteAndReport(() => this.Apply(this.Text)))
                this.OnRequestClose();
        }

        ///<summary>Check if OK Command can be executed</summary>
        bool CanOkCmd(object prm = null) {
            return true;
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
            this._okCmd = new RelayCommand(UIOperations.Apply, DoOkCmd, CanOkCmd);
            this._exitCmd = new RelayCommand(UIOperations.Exit, DoExit, (p) => true);

            ObservableCollection<CommandVModel> cmdVms = new ObservableCollection<CommandVModel>();
            cmdVms.Add(new CommandVModel(OkCmd) { Name = "OK", Description = "Apply the result" });
            cmdVms.Add(new CommandVModel(ExitCommand) { Name="Cancel", Description="Cancel" });
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


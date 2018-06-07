using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {

    ///<summary>View Model for Media Rename operation</summary>
    public class MediaRenameVModel : WorkspaceViewModel {
        ///<summary>Transformation formula</summary>
        private string _formula;
        ///<summary>Formula processor</summary>
        private TextTransformer _processor= new TextTransformer();
        ///<summary>Items to rename</summary>
        private ObservableCollection<MediaFileRenamePreview> _items= new ObservableCollection<MediaFileRenamePreview>();
        ///<summary>Result</summary>
        private bool _isConfirmed;
        ///<summary>Text to search</summary>
        private string _searchText;
        ///<summary>Replace text</summary>
        private string _replaceText;
        ///<summary>Timer to visualise current rename configuration</summary>
        private Timer _uiTimer;
        ///<summary>Last config change</summary>
        private DateTime _lastConfigChange;
        ///<summary>Last view update to reflect config changes</summary>
        private DateTime _lastViewUpdate;

        ///<summary>Last view update to reflect config changes</summary>
        public DateTime LastViewUpdate {
            get { return this._lastViewUpdate; }
            set { this._lastViewUpdate = value; }
        }
        

        ///<summary>Last config change</summary>
        public DateTime LastConfigChange {
            get { return this._lastConfigChange; }
            set { this._lastConfigChange = value; }
        }
        

        ///<summary>Timer to visualise current rename configuration</summary>
        public Timer UiTimer {
            get { return this._uiTimer; }
            set { this._uiTimer = value; }
        }
        

        ///<summary>Replace text</summary>
        public string ReplaceText {
            get { return this._replaceText; }
            set {
                if (this._replaceText != value) {
                    this._replaceText = value;
                    this.FirePropertyChanged("ReplaceText");
                    ResetViewState();
                    this.LastConfigChange = DateTime.Now;
                    //DelayedAction<RenameConfig>.Start(1000, ConditionalTry, this.GetConfiguration());
                }
            }
        }
 

        ///<summary>Text to search</summary>
        public string SearchText {
            get { return this._searchText; }
            set {
                if (this._searchText != value) {
                    this._searchText = value;
                    this.FirePropertyChanged("SearchText");
                    ResetViewState();
                    this.LastConfigChange = DateTime.Now;
                    //DelayedAction<RenameConfig>.Start(1000, ConditionalTry, this.GetConfiguration());
                }
            }
        }
        
        

        ///<summary>Result</summary>
        public bool IsConfirmed {
            get { return this._isConfirmed; }
            set { this._isConfirmed = value; }
        }
        

        ///<summary>Items to rename</summary>
        public ObservableCollection<MediaFileRenamePreview> Items {
            get { return this._items; }
            //set { this._items = value; }
        }
        

        ///<summary>Formula processor</summary>
        public TextTransformer Processor {
            get { return this._processor; }
            //set { this._processor = value; }
        }
        

        ///<summary>Transformation formula</summary>
        public string Formula {
            get { return this._formula; }
            set {
                if (this._formula != value) {
                    this._formula = value;
                    this.FirePropertyChanged("Formula");
                    ResetViewState();
                    this.LastConfigChange = DateTime.Now;
                    //DelayedAction<RenameConfig>.Start(1000, ConditionalTry, this.GetConfiguration());
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
        /// Initializes a new instance of the <see cref="MediaRenameVModel"/> class.
        /// </summary>
        public MediaRenameVModel()
            : this("Rename Media files") {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaRenameVModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public MediaRenameVModel(string title) {
            this.Title = title;
            this.LastViewUpdate = this.LastConfigChange = DateTime.Now;
            this.UiTimer = new Timer(CheckConfigRefresh, this, 1000, 1000);
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
            this.IsConfirmed = false;
            this.OnRequestClose();
        }

        ///<summary>Execute OK Command</summary>
        void DoOkCmd(object prm = null) {
            if (this.TryConfig(this.GetConfiguration())) {
                this.IsConfirmed = true;
                this.OnRequestClose();
            }
        }

        //void ConditionalTry(RenameConfig config) {
        //    //System.Diagnostics.Debug.WriteLine("-->ConditionalTry");
        //    if (this.IsConfigEqual(config)) {
        //        TryConfig(config);
        //     }
        //}


        void CheckConfigRefresh(object o) {
            if ((this.LastViewUpdate<this.LastConfigChange)&&((DateTime.Now-this.LastConfigChange).TotalMilliseconds>1000)) {
                this.RunOnUIThread(() => TryConfig(this.GetConfiguration()));
            }
        }

        bool TryConfig(RenameConfig config) {
            System.Diagnostics.Debug.WriteLine("-->TryFormula");
            string tmp;
            this.LastViewUpdate = DateTime.Now;
            this.Status.Clear();
            if (config.IsTransform) {
                bool needFormula = !string.IsNullOrWhiteSpace(config.Formula);
                bool needReplace = !string.IsNullOrEmpty(config.SearchText);
                try {
                    Func<MediaFile, string> worker = null;
                    if (needFormula)
                        worker = this.Processor.GetEvaluator(config.Formula);
                    foreach (var itm in this.Items) {
                        tmp = null;
                        if (needFormula)
                            tmp = worker(itm.Key);
                        if (needReplace) {
                            tmp = (tmp ?? itm.Key.Title).Replace(config.SearchText, config.ReplaceText);
                        }
                        itm.Value = (tmp??string.Empty).Trim();
                    }
                    return true;
                }
                catch (ArgumentException x) {
                    this.Status.SetError(string.Format("Bad pattern: {0}", x.Message));
                }
                catch (Exception x) {
                    this.Status.SetError(string.Format("Failed to apply pattern. {0}: {1}", x.GetType().Name, x.Message));
                }
            }
            else {
                foreach (var itm in this.Items) {
                    itm.Value = null;
                }

            }
            return false;
        }

        ///<summary>Check if OK Command can be executed</summary>
        bool CanOkCmd(object prm = null) {
            return !string.IsNullOrWhiteSpace(this.Formula);
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
            cmdVms.Add(new CommandVModel(OkCmd) { Name = "OK", Description = "Proceed with renaming files" });
            cmdVms.Add(new CommandVModel(ExitCommand) { Name = "Cancel", Description = "Cancel and return without changes" });
            //cmdVms.Add(new CommandVModel(ClonetCmd) { Name = "Clone", Description = "Clone workspace" });
            CommandVModels = cmdVms;
        }

        /// <summary>
        /// Reset presentation attributes according to the current state
        /// </summary>
        void ResetViewState() {
            foreach (var cmd in this.EnumerateCommands()) cmd.Reset();
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
        /// Called when instance is disposed. 
        /// Child classes can override this method to add their own logic, e.g. remove event handlers.
        /// </summary>
        protected override void OnDispose() {
            base.OnDispose();
            this.UiTimer.Dispose();
        }

        /// <summary>
        /// Adds the media files.
        /// </summary>
        /// <param name="source">The source.</param>
        public void AddMediaFiles(IEnumerable<MediaFile> source) {
            if (source != null) {
                foreach (var mf in source) {
                    this.Items.Add(new MediaFileRenamePreview() { Key = mf });
                }
            }
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns></returns>
        public RenameConfig GetConfiguration() {
            return new RenameConfig() {
                Formula= this.Formula??string.Empty,
                SearchText = this.SearchText ?? string.Empty,
                ReplaceText = this.ReplaceText ?? string.Empty
            };
        }

        /// <summary>
        /// Sets the configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public void SetConfiguration(RenameConfig config) {
            if (config == null) {
                this.Formula = this.SearchText = this.ReplaceText = string.Empty;
            }
            else {
                this.Formula = config.Formula;
                this.SearchText = config.SearchText;
                this.ReplaceText = config.ReplaceText;
            }
        }

        //bool IsConfigEqual(RenameConfig config) {
        //    return (string.Compare(this.Formula ?? string.Empty, config.Formula, true) == 0) &&
        //        (string.Compare(this.SearchText ?? string.Empty, config.SearchText, true) == 0) &&
        //        (string.Compare(this.ReplaceText ?? string.Empty, config.ReplaceText, true) == 0);
        //}

        /// <summary>
        /// Rename operation configuration
        /// </summary>
        public class RenameConfig {
            /// <summary>
            /// Gets or sets the formula.
            /// </summary>
            /// <value>
            /// The formula.
            /// </value>
            public string Formula { get; set; }
            /// <summary>
            /// Gets or sets the search text.
            /// </summary>
            /// <value>
            /// The search text.
            /// </value>
            public string SearchText { get; set; }
            /// <summary>
            /// Gets or sets the replace text.
            /// </summary>
            /// <value>
            /// The replace text.
            /// </value>
            public string ReplaceText { get; set; }

            /// <summary>
            /// Gets a value indicating whether this instance is transformation (i.e. has active formula or replace parameters).
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is transform; otherwise, <c>false</c>.
            /// </value>
            public bool IsTransform {
                get {
                    return !string.IsNullOrWhiteSpace(this.Formula) ||
                        !string.IsNullOrEmpty(this.SearchText);
                }
            }
        }


    }


    /// <summary>
    /// Presentation helper
    /// </summary>
    public class MediaFileRenamePreview : KeyValuePairX<MediaFile, string> {
        ///<summary>Is OK</summary>
        private bool _isOk;

        ///<summary>Is OK</summary>
        public bool IsOk {
            get { return this._isOk; }
            set {
                if (this._isOk != value) {
                    this._isOk = value;
                    this.FirePropertyChanged("IsOk");
                }
            }
        }
        
    }
}

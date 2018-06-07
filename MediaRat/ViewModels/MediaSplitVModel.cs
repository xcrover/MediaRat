using Ops.NetCoe.LightFrame;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XC.MediaRat {
 
    ///<summary>View Model for Media Split</summary>
    public class MediaSplitVModel : WorkspaceViewModel, ISourceProvider {
        ///<summary>Sources</summary>
        private ObservableCollection<MediaTrackSource> _sources= new ObservableCollection<MediaTrackSource>();
        ///<summary>Current Source</summary>
        private MediaTrackSource _currentSource;
        ///<summary>Current Target</summary>
        private TargetClip _currentTarget;
        ///<summary>Log</summary>
        private ILog _log;
        ///<summary>Available Transformations</summary>
        private ObservableCollection<ExternalCommand> _transformations;

        ///<summary>Available Transformations</summary>
        public ObservableCollection<ExternalCommand> Transformations {
            get { return this._transformations; }
            set {
                if (this._transformations != value) {
                    this._transformations = value;
                    this.FirePropertyChanged("Transformations");
                }
            }
        }


        ///<summary>Log</summary>
        public ILog Log {
            get { return this._log??(this._log=AppContext.Current.GetServiceViaLocator<ILog>()); }
            set { this._log = value; }
        }


        ///<summary>Current Target</summary>
        public TargetClip CurrentTarget {
            get { return this._currentTarget; }
            set {
                if (this._currentTarget != value) {
                    this._currentTarget = value;
                    this.FirePropertyChanged("CurrentTarget");
                    this.ResetViewState();
                }
            }
        }


        ///<summary>Current Source</summary>
        public MediaTrackSource CurrentSource {
            get { return this._currentSource; }
            set {
                if (this._currentSource != value) {
                    this._currentSource = value;
                    this.FirePropertyChanged("CurrentSource");
                    this.FirePropertyChanged("ActiveSource");
                    this.ResetViewState();
                }
            }
        }

        public ISourceRef ActiveSource  {
            get { return this._currentSource; }
        }

        ///<summary>Sources</summary>
        public ObservableCollection<MediaTrackSource> Sources {
            get { return this._sources; }
            //set { this._sources = value; }
        }

        #region Commands
        ///<summary>Command VModels</summary>
        private ObservableCollection<CommandVModel> _commandVModels;
        ///<summary>Exit Command</summary>
        private RelayCommand _exitCmd;
        ///<summary>Add Source Command</summary>
	    private RelayCommand _addSrcCmd;
        ///<summary>Remove Source Command</summary>
	    private RelayCommand _delSrcCmd;
        ///<summary>Run batch Command</summary>
	    private RelayCommand _executeCmd;
        ///<summary>Target command VModels</summary>
        private ObservableCollection<CommandVModel> _trgCmdVModels;
        ///<summary>Add target Command</summary>
	    private RelayCommand _addTrgCmd;
        ///<summary>Remove target Command</summary>
	    private RelayCommand _delTrgCmd;
        ///<summary>Launch async Command</summary>
	    private RelayCommand _startCmd;
        ///<summary>Get text config Command</summary>
	    private RelayCommand _getTextCfgCmd;

        ///<summary>Get text config Command</summary>
        public RelayCommand GetTextCfgCmd {
            get { return this._getTextCfgCmd; }
        }

        ///<summary>Remove target Command</summary>
        public RelayCommand DelTrgCmd {
            get { return this._delTrgCmd; }
        }

        ///<summary>Add target Command</summary>
        public RelayCommand AddTrgCmd {
            get { return this._addTrgCmd; }
        }

        ///<summary>Target command VModels</summary>
        public ObservableCollection<CommandVModel> TrgCmdVModels {
            get { return this._trgCmdVModels; }
            set {
                if (this._trgCmdVModels != value) {
                    this._trgCmdVModels = value;
                    this.FirePropertyChanged("TrgCmdVModels");
                }
            }
        }

        ///<summary>Run batch Command</summary>
        public RelayCommand ExecuteCmd {
            get { return this._executeCmd; }
        }

        ///<summary>Launch async Command</summary>
        public RelayCommand StartCmd {
            get { return this._startCmd; }
        }

        ///<summary>Remove Source Command</summary>
        public RelayCommand DelSrcCmd {
            get { return this._delSrcCmd; }
        }

        ///<summary>Add Source Command</summary>
        public RelayCommand AddSrcCmd {
            get { return this._addSrcCmd; }
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
        /// Initializes a new instance of the <see cref="MediaSplitVModel"/> class.
        /// </summary>
        public MediaSplitVModel() : this("Transform") {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaSplitVModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public MediaSplitVModel(string title) {
            this.Title = title;
            Init();
            this.InitCommands();
        }

        /// <summary>
        /// Initialize business content.
        /// This method can be called more than once
        /// </summary>
        void Init() {
            this.Status= new StatusVModel();
            PopulateTransformations();
        }

        void PopulateTransformations() {
            List<ExternalCommand> tmp = new List<ExternalCommand>();
            ExternalCommand xcmd;
            foreach (var xci in (from ci in AppContext.Current.EnumerateAppCfgItems((k) => k.StartsWith("xcmd:")|| k.StartsWith("tcmd:")) orderby ci.Code select ci)) {
                try {
                    xcmd = CreateXCmd(xci.Code, xci.Value);
                    tmp.Add(xcmd);
                }
                catch (Exception x) {
                    string em = x.ToShortMsg("Initialize External command " + xci.Code ?? "n/a");
                    Log.LogTechError(em, x);
                    this.Status.SetError(em);
                }
            }
            this.Transformations = new ObservableCollection<ExternalCommand>(from r in tmp orderby r.Title select r);
        }

        ExternalCommand CreateXCmd(string code, string config) {
            ExternalCommand xcmd;
            xcmd = ExternalCommand.CreateByCfg(config);
            xcmd.Code = code;
            return xcmd;
        }

        #endregion

        #region Command plumbing
        void DoExit(object prm) {
            this.OnRequestClose();
        }

        ///<summary>Execute Add Source Command</summary>
        void DoAddSrcCmd(object prm = null) {
            this.Status.Clear();
            FileAccessRequest far = new FileAccessRequest();
            far.IsForReading = true;
            far.IsMultiSelect = false;
            far.ExtensionFilter = "Video files (*.mts;*.mp4;*.mov;*.avi)|*.mts;*.mp4;*.mov;*.avi|Audio Files (*.mp3;*.m4a)|*.mp3;*.m4a|All files (*.*)|*.*";
            far.ExtensionFilterIndex = 1;
            InformationRequest irq = new InformationRequest() {
                ResultType = typeof(System.IO.File),
                Prompt = "Select source media file",
                Tag = far
            };
            string sourceFileName = null;
            irq.CompleteMethod = (rq) => {
                if (rq.Result != null) sourceFileName = rq.Result.ToString();
            };
            UIBus uiBus = AppContext.Current.GetServiceViaLocator<UIBus>();
            uiBus.Send(new UIMessage(this, UIBusMessageTypes.InformationRequest, irq));
            AddMediaSource(sourceFileName);
        }

        ///<summary>Check if Add Source Command can be executed</summary>
        bool CanAddSrcCmd(object prm = null) {
            return true;
        }


        ///<summary>Execute Remove Source Command</summary>
        void DoDelSrcCmd(object prm = null) {
            this.Status.Clear();
        }

        ///<summary>Check if Remove Source Command can be executed</summary>
        bool CanDelSrcCmd(object prm = null) {
            return true;
        }


        ///<summary>Execute Run batch Command</summary>
        void DoExecuteCmd(object prm = null) {
            this.Status.Clear();
            this.ExecuteAndReport(() => {
                Process((MediaTrackSource)this.CurrentSource.Clone());
                this.Status.SetPositive("Done");
            });
        }

        ///<summary>Check if Run batch Command can be executed</summary>
        bool CanExecuteCmd(object prm = null) {
            return true;
        }

        ///<summary>Execute Add target Command</summary>
        void DoAddTrgCmd(object prm = null) {
            this.Status.Clear();
            this.CurrentSource.Targets.Add(new TargetClip());
        }

        ///<summary>Check if Add target Command can be executed</summary>
        bool CanAddTrgCmd(object prm = null) {
            return this.CurrentSource!=null;
        }

        ///<summary>Execute Remove target Command</summary>
        void DoDelTrgCmd(object prm = null) {
            this.Status.Clear();
            this.CurrentSource.Targets.Remove(this.CurrentTarget);
            this.CurrentTarget = null;
        }

        ///<summary>Check if Remove target Command can be executed</summary>
        bool CanDelTrgCmd(object prm = null) {
            return (this.CurrentSource!= null) && (this.CurrentTarget!=null);
        }


        ///<summary>Execute Launch async Command</summary>
        void DoStartCmd(object prm = null) {
            this.Status.Clear();
            Task.Factory.StartNew(() => {
                this.ExecuteAndReport(() => {
                    Process((MediaTrackSource)this.CurrentSource.Clone());
                    this.RunOnUIThread(()=>this.Status.SetPositive("Done"));
                });
            });
        }

        ///<summary>Check if Launch async Command can be executed</summary>
        bool CanStartCmd(object prm = null) {
            return true;
        }


        ///<summary>Execute Get text config Command</summary>
        void DoGetTextCfgCmd(object prm = null) {
            var ui = AppContext.Current.GetServiceViaLocator<IUIHelper>();
            ui.TryAskText("Track configuration", "Track config (HH:MM:SS Tilte):", string.Empty, ApplyTextCfg);
        }

        void ApplyTextCfg(string src) {
            string[] lines = src.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int ix;
            string ln, tm;
            TargetClip tcl, pcl=null;
            TrackTimeT trt;
            List<TargetClip> rz = new List<TargetClip>();
            TimeSpan tsp;
            double gapSec = 2;

            foreach(var ts in lines) {
                ln = ts.Trim();
                ix = ln.IndexOf(' ');
                if (ix < 0) continue;
                tm = ln.Substring(0, ix);
                if (TryParseTS(tm, out tsp)) {
                    tcl = new TargetClip() {
                       TargetFile= "+-"+ln.Substring(ix+1).Trim(),
                        SourceFrame= new TrackTimeT() {
                             Start= tsp
                        }
                    };
                    if (pcl!=null) {
                        pcl.SourceFrame.Stop = tcl.SourceFrame.Start.Value - TimeSpan.FromSeconds(gapSec);
                    }
                    rz.Add(tcl);
                    pcl = tcl; // prev clip
                }
            }
            if (this.CurrentSource!= null) {
                foreach (var t in rz)
                    this.CurrentSource.Targets.Add(t);
            }
        }

        bool TryParseTS(string ts, out TimeSpan tsp) {
            try {
                string[] hms = ts.Split(':');
                switch (hms.Length) {
                    case 0: tsp = TimeSpan.Zero; return true;
                    case 1: tsp = TimeSpan.FromSeconds(double.Parse(hms[0])); return true;
                    case 2: tsp = new TimeSpan(0, int.Parse(hms[0]), int.Parse(hms[1])); return true;
                    default: tsp = new TimeSpan(int.Parse(hms[0]), int.Parse(hms[1]), int.Parse(hms[2])); return true;
                }
            }
            catch {
                tsp = TimeSpan.Zero;
                return false;
            }
        }

        ///<summary>Check if Get text config Command can be executed</summary>
        bool CanGetTextCfgCmd(object prm = null) {
            return true;
        }

        /// <summary>
        /// Enumerate all the available commands
        /// </summary>
        IEnumerable<RelayCommand> EnumerateCommands() {
            yield return this.AddSrcCmd;
            yield return this.DelSrcCmd;
            yield return this.AddTrgCmd;
            yield return this.DelTrgCmd;
            yield return this.ExecuteCmd;
            yield return this.StartCmd;
            yield return this.GetTextCfgCmd;
            yield return this.ExitCommand;
        }

        /// <summary>
        /// Initialize commands infrastructure
        /// </summary>
        void InitCommands() {
            this._addSrcCmd = new RelayCommand(UIOperations.AddSource, DoAddSrcCmd, CanAddSrcCmd);
            this._delSrcCmd = new RelayCommand(UIOperations.Delete, DoDelSrcCmd, CanDelSrcCmd);
            this._addTrgCmd = new RelayCommand(UIOperations.AddItem, DoAddTrgCmd, CanAddTrgCmd);
            this._delTrgCmd = new RelayCommand(UIOperations.DeleteItem, DoDelTrgCmd, CanDelTrgCmd);
            this._executeCmd = new RelayCommand(UIOperations.Apply, DoExecuteCmd, CanExecuteCmd);
            this._startCmd = new RelayCommand(UIOperations.ApplyPropElements, DoStartCmd, CanStartCmd);
            this._getTextCfgCmd = new RelayCommand(UIOperations.AddSource, DoGetTextCfgCmd, CanGetTextCfgCmd);
            this._exitCmd = new RelayCommand(UIOperations.Exit, DoExit, (p) => true);

            ObservableCollection<CommandVModel> cmdVms = new ObservableCollection<CommandVModel>();
            cmdVms.Add(new CommandVModel(AddSrcCmd) { Name = "Add Source", Description = "Add Source" });
            cmdVms.Add(new CommandVModel(DelSrcCmd) { Name = "Delete", Description = "Remove Source" });
            cmdVms.Add(new CommandVModel(ExecuteCmd) { Name = "Run", Description = "Run batch" });
            cmdVms.Add(new CommandVModel(StartCmd) { Name = "StartA", Description = "Run batch asynchronously" });
            cmdVms.Add(new CommandVModel(GetTextCfgCmd) { Name = "From text", Description = "Get text configuration (start points)" });
            cmdVms.Add(new CommandVModel(ExitCommand));
            //cmdVms.Add(new CommandVModel(ClonetCmd) { Name = "Clone", Description = "Clone workspace" });
            CommandVModels = cmdVms;

            cmdVms = new ObservableCollection<CommandVModel>();
            cmdVms.Add(new CommandVModel(AddTrgCmd) { Name = "Add", Description = "Add target" });
            cmdVms.Add(new CommandVModel(DelTrgCmd) { Name = "Delete", Description = "Remove target" });
            this.TrgCmdVModels = cmdVms;
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

        #region Operations

        public void AddMediaSource(string filePath) {
            if (!string.IsNullOrEmpty(filePath)) {
                MediaTrackSource mts = new MediaTrackSource() {
                    MediaType = MediaTypes.Video,
                    SourcePath = filePath
                };
                this.CurrentSource = mts;
            }
        }

        public void ProcessDroppedFiles(IEnumerable<string> files) {
            if (files != null)
                AddMediaSource(files.FirstOrDefault());
        }

        public void Process(MediaTrackSource msrc) {
            this.Status.Clear();
            ExternalCommand xcmd;
            if (msrc.Targets.Count > 0) {
                List<CodeValuePair> srcPrms = new List<CodeValuePair>(TemplParams.EnumerateStdSourceParams(msrc));
                foreach (var t in msrc.Targets) {
                    t.Source = msrc;
                    xcmd = t.XCmd;
                    if ((xcmd!=null)&&xcmd.CanExecute(msrc)) {
                        var pQ = srcPrms.Union(EnumerateTrgParams(t));
                        xcmd.Execute(pQ);
                    }
                }
            }
        }

        IEnumerable<CodeValuePair> EnumerateTrgParams(TargetClip trg) {
            string tfn;
            if (!string.IsNullOrEmpty(trg.TargetFile)) {
                if (trg.TargetFile.StartsWith("+")) {
                    tfn = System.IO.Path.GetFileNameWithoutExtension(trg.Source.SourcePath) + trg.TargetFile.Substring(1);
                }
                else {
                    tfn = trg.TargetFile;
                }
            }
            else {
                tfn = System.IO.Path.GetFileNameWithoutExtension(trg.Source.SourcePath);
            }
            yield return new CodeValuePair() { Code = TemplParams.TrgFileName, Value = tfn };
            yield return new CodeValuePair() { Code = TemplParams.TrgDirPath, Value= System.IO.Path.GetDirectoryName(trg.Source.SourcePath) };
            yield return new CodeValuePair() { Code = TemplParams.TimeStart, Value = ToTimeCodeStr(trg.SourceFrame.Start) };
            yield return new CodeValuePair() { Code = TemplParams.TimeStop, Value = ToTimeCodeStr(trg.SourceFrame.Stop) };
            yield return new CodeValuePair() { Code = TemplParams.TimeDuration, Value = ToTimeCodeStr(trg.SourceFrame.Duration) };
        }


        string ToTimeCodeStr(TimeSpan? ts) {
            if (ts.HasValue)
                return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Value.Hours, ts.Value.Minutes, ts.Value.Seconds, ts.Value.Milliseconds);
            else
                return "00:00:00";
        }
        #endregion
    }

    public class MediaTrackSource : NotifyPropertyChangedBase, ISourceRef, ICloneable {
        ///<summary>Source file path</summary>
        private string _sourcePath;
        ///<summary>Media Type</summary>
        private MediaTypes _mediaType;
        ///<summary>Target Clips</summary>
        private ObservableCollection<TargetClip> _targets= new ObservableCollection<TargetClip>();

        ///<summary>Target Clips</summary>
        public ObservableCollection<TargetClip> Targets {
            get { return this._targets; }
            //set { this._targets = value; }
        }


        ///<summary>Media Type</summary>
        public MediaTypes MediaType {
            get { return this._mediaType; }
            set {
                if (this._mediaType != value) {
                    this._mediaType = value;
                    this.FirePropertyChanged("MediaType");
                }
            }
        }


        ///<summary>Source file path</summary>
        public string SourcePath {
            get { return this._sourcePath; }
            set {
                if (this._sourcePath != value) {
                    this._sourcePath = value;
                    this.FirePropertyChanged("SourceFile");
                }
            }
        }

        public object Clone() {
            MediaTrackSource rz = new MediaTrackSource() {
                _mediaType = this.MediaType,
                _sourcePath = this.SourcePath,
                _targets = new ObservableCollection<TargetClip>(this.Targets)
            };
            return rz;
        }
    }

    public class TargetClip : NotifyPropertyChangedBase {
        ///<summary>Target file path</summary>
        private string _targetFile;
        ///<summary>Media Type</summary>
        private MediaTypes _mediaType;
        ///<summary>Source Track</summary>
        private MediaTrackSource _source;
        ///<summary>Source timeframe</summary>
        private TrackTimeT _sourceFrame = new TrackTimeT();
        ///<summary>Command to execute</summary>
        private ExternalCommand _xCmd;

        ///<summary>Command to execute</summary>
        public ExternalCommand XCmd {
            get { return this._xCmd; }
            set {
                if (this._xCmd != value) {
                    this._xCmd = value;
                    this.FirePropertyChanged("XCmd");
                }
            }
        }


        ///<summary>Source timeframe</summary>
        public TrackTimeT SourceFrame {
            get { return this._sourceFrame; }
            set {
                if (this._sourceFrame != value) {
                    this._sourceFrame = value;
                    this.FirePropertyChanged("SourceFrame");
                }
            }
        }


        ///<summary>Source Track</summary>
        public MediaTrackSource Source {
            get { return this._source; }
            set {
                if (this._source != value) {
                    this._source = value;
                    this.FirePropertyChanged("Source");
                }
            }
        }


        ///<summary>Media Type</summary>
        public MediaTypes MediaType {
            get { return this._mediaType; }
            set {
                if (this._mediaType != value) {
                    this._mediaType = value;
                    this.FirePropertyChanged("MediaType");
                }
            }
        }


        ///<summary>Target file path</summary>
        public string TargetFile {
            get { return this._targetFile; }
            set {
                if (this._targetFile != value) {
                    this._targetFile = value;
                    this.FirePropertyChanged("TargetFile");
                }
            }
        }

    }
}

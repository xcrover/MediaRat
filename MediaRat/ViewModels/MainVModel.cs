using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml.Linq;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
    ///<summary>View Model for Main View</summary>
    public class MainVModel : WorkspaceContainerViewModel {
        #region Privats:)
        ///<summary>Log View Model</summary>
        private LogVModel _logVm;
        ///<summary>Module version</summary>
        private string _moduleVersion;

        #endregion

        #region Commands
        ///<summary>Command VModels</summary>
        private ObservableCollection<CommandVModel> _commandVModels;
        ///<summary>Exit Command</summary>
        private RelayCommand _exitCmd;
        ///<summary>New Project Command</summary>
        private RelayCommand _newProjectCmd;
        ///<summary>Open project Command</summary>
        private RelayCommand _openProjectCmd;
        ///<summary>New Video Project Command</summary>
        private RelayCommand _newVideoProjectCmd;
        ///<summary>Media Split Command</summary>
	    private RelayCommand _mediaSplitCmd;

        ///<summary>Media Split Command</summary>
        public RelayCommand MediaSplitCmd {
            get { return this._mediaSplitCmd; }
        }

        ///<summary>New Video Project Command</summary>
        public RelayCommand NewVideoProjectCmd {
            get { return this._newVideoProjectCmd; }
        }

         
        ///<summary>Open project Command</summary>
        public RelayCommand OpenProjectCmd {
            get { return this._openProjectCmd; }
        }

        ///<summary>New Project Command</summary>
        public RelayCommand NewProjectCmd {
            get { return this._newProjectCmd; }
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
        /// Initializes a new instance of the <see cref="MainVModel"/> class.
        /// </summary>
        public MainVModel()
            : this("Media Rat") {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainVModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public MainVModel(string title) {
            this.ModuleVersion = this.GetType().Assembly.GetName().Version.ToString();
            this.Title = title + " " + this.ModuleVersion;
            Init();
            this.InitCommands();
            this.StartUp();
        }

        /// <summary>
        /// Initialize business content.
        /// This method can be called more than once
        /// </summary>
        void Init() {
            this.Status = new StatusVModel();
            this._logVm = AppContext.Current.GetServiceViaLocator<LogVModel>();
        }

        void StartUp() {
            string inFile = "n/a";
            try {
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length>1) { // #0 is exe path
                    inFile = args[1];
                    OpenFile(inFile);
                }
                //StartupOptions sop = new StartupOptions();
                //string inFile = sop["i"];
                //if (inFile!=null) {
                //    OpenFile(inFile);
                //}
            }
            catch (Exception x) {
                this.ReportError(x);
            }
        }
        #endregion

        #region Properties

        ///<summary>Log View Model</summary>
        public LogVModel LogVm {
            get { return this._logVm; }
            set {
                if (this._logVm != value) {
                    this._logVm = value;
                    this.FirePropertyChanged("LogVm");
                }
            }
        }

        ///<summary>Module version</summary>
        public string ModuleVersion {
            get { return this._moduleVersion; }
            set {
                if (this._moduleVersion != value) {
                    this._moduleVersion = value;
                    this.FirePropertyChanged("ModuleVersion");
                }
            }
        }

        #endregion

        #region Command plumbing
        void DoExit(object prm) {
            this.OnRequestClose();
            //if (this.GetCloseConfirmation()) {
            //    this.OnRequestClose();
            //}
        }

        void AddDefaultSettings(MediaProject prj) {
            RatingDefinition rd = new RatingDefinition() {
                Marker = "q",
                Title = "Technical quality",
                Description = "Overall technical quality (sharpnes, color balance, light etc.)"
            };
            prj.RatingDefinitions.Add(rd);
            rd = new RatingDefinition() {
                Marker = "x",
                Title = "Excitement Level",
                Description = "How interesting is content"
            };
            prj.RatingDefinitions.Add(rd);
            CategoryDefinition cd = new CategoryDefinition() {
                Marker = "content",
                Title="Content",
                Description= "Description of the media content"
            };
            prj.CategoryDefinitions.Add(cd);
            cd = new CategoryDefinition() {
                Marker = "publish",
                Title = "Publish to",
                Description = "Where this media should be published",
                Values = new ObservableCollection<string>() { "Print", "SShow1", "SShow2", "Trash"}
            };
            prj.CategoryDefinitions.Add(cd);
        }

        ///<summary>Execute New Project Command</summary>
        void DoNewProjectCmd(object prm = null) {
            ImageProjectVModel vm = new ImageProjectVModel() {
                Entity = new MediaProject()
            };
            this.AddDefaultSettings(vm.Entity);
            this.EnsureWorkspace(null, () => vm);
        }

        ///<summary>Check if New Project Command can be executed</summary>
        bool CanNewProjectCmd(object prm = null) {
            return true;
        }


        ///<summary>Execute Open project Command</summary>
        void DoOpenProjectCmd(object prm = null) {
            FileAccessRequest far = new FileAccessRequest();
            far.IsForReading = true;
            far.IsMultiSelect = false;
            far.ExtensionFilter = "Media Rat project (*.xmr)|*.xmr|Media Rat project (*.xmv)|*.xmv|XML Documents (*.xml)|*.xml|All files (*.*)|*.*";
            far.ExtensionFilterIndex = 1;
            InformationRequest irq = new InformationRequest() {
                ResultType = typeof(System.IO.File),
                Prompt = "Select Media Rat project file",
                Tag = far
            };
            string sourceFileName = null;
            irq.CompleteMethod = (rq) => {
                if (rq.Result != null) sourceFileName = rq.Result.ToString();
            };
            UIBus uiBus = AppContext.Current.GetServiceViaLocator<UIBus>();
            uiBus.Send(new UIMessage(this, UIBusMessageTypes.InformationRequest, irq));
            OpenFile(sourceFileName);
        }

        void OpenFile(string sourceFileName) {
            if (sourceFileName != null) {
                string ext = System.IO.Path.GetExtension(sourceFileName).ToLower();
                switch (ext) {
                    case ".xmv":
                        VideoProjectVModel vvm = new VideoProjectVModel();
                        this.EnsureWorkspace(null, () => vvm);
                        vvm.LoadProject(sourceFileName);
                        break;
                    default:
                        ImageProjectVModel vm = new ImageProjectVModel();
                        this.EnsureWorkspace(null, () => vm);
                        vm.LoadProject(sourceFileName);
                        break;
                }
            }
        }

        ///<summary>Check if Open project Command can be executed</summary>
        bool CanOpenProjectCmd(object prm = null) {
            return true;
        }

        ///<summary>Execute New Video Project Command</summary>
        void DoNewVideoProjectCmd(object prm = null) {
            VideoProjectVModel vm = new VideoProjectVModel() {
                Entity = new VideoProject()
            };
            this.EnsureWorkspace(null, () => vm);
        }

        ///<summary>Check if New Video Project Command can be executed</summary>
        bool CanNewVideoProjectCmd(object prm = null) {
            return true;
        }


        ///<summary>Execute Media Split Command</summary>
        void DoMediaSplitCmd(object prm = null) {
            MediaSplitVModel vm = new MediaSplitVModel();
            this.EnsureWorkspace(null, () => vm);
        }

        ///<summary>Check if Media Split Command can be executed</summary>
        bool CanmediaSplitCmd(object prm = null) {
            return true;
        }

        /// <summary>
        /// Enumerate all the available commands
        /// </summary>
        IEnumerable<RelayCommand> EnumerateCommands() {
            yield return this.NewProjectCmd;
            yield return this.NewVideoProjectCmd;
            yield return this.OpenProjectCmd;
            yield return this.MediaSplitCmd;
            yield return this.ExitCommand;
        }

        /// <summary>
        /// Initialize commands infrastructure
        /// </summary>
        void InitCommands() {
            this._newProjectCmd = new RelayCommand(UIOperations.AddItem, DoNewProjectCmd, CanNewProjectCmd);
            this._newVideoProjectCmd = new RelayCommand(UIOperations.AddItem, DoNewVideoProjectCmd, CanNewVideoProjectCmd);
            this._openProjectCmd = new RelayCommand(UIOperations.Open, DoOpenProjectCmd, CanOpenProjectCmd);
            this._mediaSplitCmd = new RelayCommand(UIOperations.Rename, DoMediaSplitCmd, CanmediaSplitCmd);
            this._exitCmd = new RelayCommand(UIOperations.Exit, DoExit, (p) => true);

            ObservableCollection<CommandVModel> cmdVms = new ObservableCollection<CommandVModel>();
            cmdVms.Add(new CommandVModel(NewProjectCmd) { Name = "New Rating", Description = "Create new Rating Project" });
            cmdVms.Add(new CommandVModel(NewVideoProjectCmd) { Name = "New Video", Description = "Create new Video Project" });
            cmdVms.Add(new CommandVModel(OpenProjectCmd) { Name = "Open", Description = "Open existing project" });
            cmdVms.Add(new CommandVModel(MediaSplitCmd) { Name = "Transform", Description = "Transform Media" });
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
        /// Gets the close confirmation.
        /// </summary>
        /// <returns></returns>
        public bool GetCloseConfirmation() {
            if (this.Workspaces.Count ==0 ) return true;
            var rz= this.Locator.GetService<IUIHelper>().GetUserConfirmation("You are closing the application with at least one open project."+
                    " Probably some information has not be saved.\r\n\r\n\tContinue?");
            this.OnDispose();
            return rz;
        }

     }
}

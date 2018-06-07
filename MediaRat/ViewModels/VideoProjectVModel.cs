using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Ops.NetCoe.LightFrame;
using System.Text.RegularExpressions;

namespace XC.MediaRat {
 
    ///<summary>View Model for Video Project</summary>
    public class VideoProjectVModel : WorkspaceViewModel {
        ///<summary>Video Project</summary>
        private VideoProject _entity;
        ///<summary>View access</summary>
        private IVProjectView _viewUtil;
        ///<summary>Root track list VModel</summary>
        private TrackListVModel _rootTracksVModel= new TrackListVModel();

        ///<summary>Root track list VModel</summary>
        public TrackListVModel RootTracksVModel {
            get { return this._rootTracksVModel; }
            set { this._rootTracksVModel = value; }
        }
        

        ///<summary>View access</summary>
        public IVProjectView ViewUtil {
            get { return this._viewUtil; }
            set {
                if (this._viewUtil != value) {
                    this._viewUtil = value;
                    this.FirePropertyChanged("ViewUtil");
                }
            }
        }
        

        ///<summary>Video Project</summary>
        public VideoProject Entity {
            get { return this._entity; }
            set {
                if (this._entity != value) {
                    this._entity = value;
                    this.FirePropertyChanged("Entity");
                    this.RootTracksVModel.Tracks = (value == null) ? null : value.Root.Tracks;
                }
            }
        }
        

        #region Commands
        ///<summary>Command VModels</summary>
        private ObservableCollection<CommandVModel> _commandVModels;
        ///<summary>Exit Command</summary>
        private RelayCommand _exitCmd;
        ///<summary>Save Command</summary>
        private RelayCommand _saveCmd;
        ///<summary>Save As Command</summary>
        private RelayCommand _saveAsCmd;
        ///<summary>Create new track Command</summary>
        private RelayCommand _createTrackCmd;
        ///<summary>Create Track Group Command</summary>
        private RelayCommand _createTrackGrpCmd;

        ///<summary>Create Track Group Command</summary>
        public RelayCommand createTrackGrpCmd {
            get { return this._createTrackGrpCmd; }
        }


        ///<summary>Create new track Command</summary>
        public RelayCommand CreateTrackCmd {
            get { return this._createTrackCmd; }
        }

        ///<summary>Save As Command</summary>
        public RelayCommand SaveAsCmd {
            get { return this._saveAsCmd; }
        }

        ///<summary>Save Command</summary>
        public RelayCommand SaveCmd {
            get { return this._saveCmd; }
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
        /// Initializes a new instance of the <see cref="VideoProjectVModel"/> class.
        /// </summary>
        public VideoProjectVModel()
            : this("VideoProjectVModel") {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoProjectVModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public VideoProjectVModel(string title) {
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
            this.RootTracksVModel.Status = this.Status;
        }

        public void LoadProject(string sourcePath) {
            this.ExecuteAndReport(() => this.Entity = VideoProject.Load(sourcePath));
        }
        #endregion

        #region Command plumbing
        void DoExit(object prm) {
            if (this.Entity.IsDirty) {
                var ui = this.Locator.GetService<IUIHelper>();
                if (ui.GetUserConfirmation(string.Format("Project {0} has unsaved changes.\r\n\r\n\tWould you like save it?", this.Entity.Title))) {
                    this.SaveProject(this.Entity);
                }
            }
            this.OnRequestClose();
        }


        ///<summary>Execute Save Command</summary>
        void DoSaveCmd(object prm = null) {
            try {
                this.SaveProject(this.Entity);
            }
            catch (BizException bx) {
                this.Status.SetError(bx.Message);
            }
            catch (Exception x) {
                this.Status.SetError(string.Format("Faield to save project. {0}: {1}", x.GetType().Name, x.Message), x);
            }
        }

        ///<summary>Check if Save Command can be executed</summary>
        bool CanSaveCmd(object prm = null) {
            return true;
        }


        ///<summary>Execute Save As Command</summary>
        void DoSaveAsCmd(object prm = null) {
            try {
                this.SaveProject(this.Entity, true);
            }
            catch (BizException bx) {
                this.Status.SetError(bx.Message);
            }
            catch (Exception x) {
                this.Status.SetError(string.Format("Faield to save project. {0}: {1}", x.GetType().Name, x.Message), x);
            }
        }

        ///<summary>Check if Save As Command can be executed</summary>
        bool CanSaveAsCmd(object prm = null) {
            return true;
        }

        void Test1() {
            //Regex rgx = new Regex(@"\bOU=(.*?),", RegexOptions.None);
            Regex rgx = new Regex(@"\b(.*?)=(.*?),", RegexOptions.None);
            var src = string.IsNullOrEmpty(this.Entity.Description) ? "OU=Zhopa,OU=PROD,OU=Physical" : this.Entity.Description;
            //var m = rgx.Matches(src);
            //CN=Sergey,OU=Physical,OU=Architect,DC=GOV,DC=ON,DC=CA
            HashSet<string> rz = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach(Match m in rgx.Matches(src)) {
                if (m.Groups.Count>1) {
                    rz.Add(m.Groups[1].Value);
                }
            }
            System.Diagnostics.Debug.WriteLine(string.Join("|", rz));
        }

        ///<summary>Execute Create new track Command</summary>
        void DoCreateTrackCmd(object prm = null) {
            //Test1();
            List<IContentSource> srcs = new List<IContentSource>(this.ViewUtil.EnumerateSelectedSources());
            MediaTrack mt;
            if (srcs.Count == 0) {
                this.Status.SetError("You need one or more sources selected");
            }
            foreach (var cs in srcs) {
                this.Entity.AddNewTrack(cs);
            }
        }

        ///<summary>Check if Create new track Command can be executed</summary>
        bool CanCreateTrackCmd(object prm = null) {
            return true;
        }

        IManagedView _grpEditor;

        ///<summary>Execute Create Track Group Command</summary>
        void DoCreateTrackGrpCmd(object prm = null) {
            var tg= this.Entity.AddNewTrackGroup(this.ViewUtil.EnumerateSelectedTracks());
            this.Entity.Root.Tracks.Add(tg);

            if (this._grpEditor == null) {
                TrackGroupVModel gvm = new TrackGroupVModel();
                gvm.Entity = tg;
                Views.PopupView vw= new Views.PopupView(gvm);
                vw.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                vw.Closed += (s,a)=>_grpEditor=null;
                _grpEditor = vw;
                vw.Show();
            }
            else {
                this._grpEditor.EnsureVisible();
            }
        }

        ///<summary>Check if Create Track Group Command can be executed</summary>
        bool CanCreateTrackGrpCmd(object prm = null) {
            return true;
        }

 
        /// <summary>
        /// Enumerate all the available commands
        /// </summary>
        IEnumerable<RelayCommand> EnumerateCommands() {
            yield return this.CreateTrackCmd;
            yield return this.createTrackGrpCmd;
            yield return this.SaveCmd;
            yield return this.SaveAsCmd;
            yield return this.ExitCommand;
        }

        /// <summary>
        /// Initialize commands infrastructure
        /// </summary>
        void InitCommands() {
            this._createTrackCmd = new RelayCommand(UIOperations.CreateTrack, DoCreateTrackCmd, CanCreateTrackCmd);
            this._createTrackGrpCmd = new RelayCommand(UIOperations.CreateTrackGroup, DoCreateTrackGrpCmd, CanCreateTrackGrpCmd);
            this._saveCmd = new RelayCommand(UIOperations.Save, DoSaveCmd, CanSaveCmd);
            this._saveAsCmd = new RelayCommand(UIOperations.SaveAs, DoSaveAsCmd, CanSaveAsCmd);
            this._exitCmd = new RelayCommand(UIOperations.Exit, DoExit, (p) => true);

            ObservableCollection<CommandVModel> cmdVms = new ObservableCollection<CommandVModel>();
            cmdVms.Add(new CommandVModel(CreateTrackCmd) { Name = "+Tracks", Description = "Create new track for each selected source" });
            cmdVms.Add(new CommandVModel(createTrackGrpCmd) { Name = "+Track GRP", Description = "Create Track as a group of selected tracks" });
            cmdVms.Add(new CommandVModel(SaveCmd) { Name = "Save", Description = "Save the project" });
            cmdVms.Add(new CommandVModel(SaveAsCmd) { Name = "Save As", Description = "Save project with alternative name" });
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

        void SaveProject(VideoProject prj, bool askAltName = false) {
            if (!string.IsNullOrEmpty(prj.ProjectFileName) && !askAltName) {
                prj.Save(prj.ProjectFileName);
                return;
            }
            FileAccessRequest far = new FileAccessRequest();
            far.IsForReading = false;
            far.IsMultiSelect = false;
            far.ExtensionFilter = "Media Rat Video Project (*.xmv)|*.xmv|XML Documents (*.xml)|*.xml|All files (*.*)|*.*";
            far.ExtensionFilterIndex = 1;
            far.SuggestedFileName = "MyProject.xmv";
            InformationRequest irq = new InformationRequest() {
                ResultType = typeof(System.IO.File),
                Prompt = "Save project file",
                Tag = far
            };
            string fileName = null;
            irq.CompleteMethod = (rq) => {
                if (rq.Result != null) fileName = rq.Result.ToString();
            };
            UIBus uiBus = AppContext.Current.GetServiceViaLocator<UIBus>();
            uiBus.Send(new UIMessage(this, UIBusMessageTypes.InformationRequest, irq));
            if (fileName != null) {
                prj.Save(fileName);
            }
        }

        #region Sources

        /// <summary>
        /// Adds the sources.
        /// </summary>
        /// <param name="sourceFiles">The source files.</param>
        public void AddSources(IEnumerable<string> sourceFiles) {
            this.IsBusy= true;
            this.Entity.StartCreatingMediaSources(sourceFiles).ContinueWith(t => {
                if (CheckSuccess(t)) {
                    this.RunOnUIThread(() => {
                        this.IsBusy = false;
                        this.Entity.AddMediaSources(t.Result);
                    });
                }
            });
        }

        #endregion

        #region View Communication

        /// <summary>
        /// Call back contract for View
        /// </summary>
        public interface IVProjectView {

            /// <summary>
            /// Enumerates the selected content sources.
            /// </summary>
            /// <returns></returns>
            IEnumerable<IContentSource> EnumerateSelectedSources();
            /// <summary>
            /// Enumerates the selected tracks.
            /// </summary>
            /// <returns></returns>
            IEnumerable<IMediaTrack> EnumerateSelectedTracks();

        }

        #endregion
    }

    
}

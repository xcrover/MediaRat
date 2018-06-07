using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Ops.NetCoe.LightFrame;
using System.Threading;

namespace XC.MediaRat {

    ///<summary>View Model for Media Project actions?</summary>
    public class ProjectActionsVModel : WorkspaceViewModel {
        ///<summary>Media Project</summary>
        private MediaProject _project;
        ///<summary>UI Helper</summary>
        private IUIHelper _uiHelper;
        ///<summary>Parent View Model</summary>
        private ImageProjectVModel _parent;
        ///<summary>Number of Selected Media items</summary>
        private int _setSize;
        ///<summary>Current operations</summary>
        private ObservableCollection<ActionStatusVModel> _currentActions= new ObservableCollection<ActionStatusVModel>();
        ///<summary>Logger</summary>
        private ILog _log;
        private MediaRenameVModel.RenameConfig _lastRenameConfig;

        ///<summary>Logger</summary>
        public ILog Log {
            get { return this._log; }
        }
        

        ///<summary>Current operations</summary>
        public ObservableCollection<ActionStatusVModel> CurrentActions {
            get { return this._currentActions; }
            //set {
            //    if (this._currentActions != value) {
            //        this._currentActions = value;
            //        this.FirePropertyChanged("CurrentActions");
            //    }
            //}
        }
        

        ///<summary>Number of Selected Media items</summary>
        public int SetSize {
            get { return this._setSize; }
            set {
                if (this._setSize != value) {
                    this._setSize = value;
                    this.FirePropertyChanged("SetSize");
                    this.ResetViewState();
                }
            }
        }
        

        ///<summary>Parent View Model</summary>
        public ImageProjectVModel Parent {
            get { return this._parent; }
            protected set {
                if (this._parent != value) {
                    if (this._parent != null) {
                        this._parent.PropertyChanged -= _parent_PropertyChanged;
                    }
                    this._parent = value;
                    if (this._parent != null) {
                        this._parent.PropertyChanged += _parent_PropertyChanged;
                    }
                }
            }
        }

        void _parent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "Entity": this.Project = this.Parent.Entity; break;
                case "MediaItems": {
                        var src= this.GetSelectedMedia();
                        this.SetSize = (src == null) ? 0 : src.Count;
                        break;
                    }
            }
        }
        

        ///<summary>UI Helper</summary>
        public IUIHelper UIHelper {
            get { return this._uiHelper; }
            set { this._uiHelper = value; }
        }
        

        ///<summary>Media Project</summary>
        public MediaProject Project {
            get { return this._project; }
            set {
                if (this._project != value) {
                    this._project = value;
                    this.FirePropertyChanged("project");
                }
            }
        }
        
        #region Commands
        ///<summary>Command VModels</summary>
        private ObservableCollection<CommandVModel> _commandVModels;
        ///<summary>Copy Media files to the specified folder Command</summary>
        private RelayCommand _copyMediaToFolderCmd;
        ///<summary>Rename media files Command</summary>
        private RelayCommand _renameMediaCmd;
        ///<summary>Move Media Files Command</summary>
        private RelayCommand _moveMediaCmd;
        ///<summary>Mark all selected media items Command</summary>
        private RelayCommand _markAllCmd;
        ///<summary>Unmark all selected media items Command</summary>
        private RelayCommand _unMarkAllCmd;
        ///<summary>Inverse mark on selected items Command</summary>
        private RelayCommand _inverseMarkCmd;
        ///<summary>Remove from project Command</summary>
        private RelayCommand _excludeCmd;
        ///<summary>Export to Movie Maker Project Command</summary>
        private RelayCommand _movMakerProjectCmd;
        ///<summary>Export media sheet Command</summary>
        private RelayCommand _exportMediaSheetCmd;
        ///<summary>Populate media properties Command</summary>
        private RelayCommand _populateMediaPropsCmd;
        ///<summary>Slide show Command</summary>
	    private RelayCommand _slideShowCmd;

        ///<summary>Slide show Command</summary>
        public RelayCommand SlideShowCmd {
            get { return this._slideShowCmd; }
        }

        ///<summary>Export to Movie Maker Project Command</summary>
        public RelayCommand MovMakerProjectCmd {
            get { return this._movMakerProjectCmd; }
        }

        ///<summary>Move Media Files Command</summary>
        public RelayCommand MoveMediaCmd {
            get { return this._moveMediaCmd; }
        }
        
        ///<summary>Rename media files Command</summary>
        public RelayCommand RenameMediaCmd {
            get { return this._renameMediaCmd; }
        }
        
        ///<summary>Copy Media files to the specified folder Command</summary>
        public RelayCommand CopyMediaToFolderCmd {
            get { return this._copyMediaToFolderCmd; }
        }

        ///<summary>Mark all selected media items Command</summary>
        public RelayCommand MarkAllCmd {
            get { return this._markAllCmd; }
        }

        ///<summary>Unmark all selected media items Command</summary>
        public RelayCommand UnMarkAllCmd {
            get { return this._unMarkAllCmd; }
        }

        ///<summary>Inverse mark on selected items Command</summary>
        public RelayCommand InverseMarkCmd {
            get { return this._inverseMarkCmd; }
        }

        ///<summary>Remove from project Command</summary>
        public RelayCommand ExcludeCmd {
            get { return this._excludeCmd; }
        }

        ///<summary>Export media sheet Command</summary>
        public RelayCommand ExportMediaSheetCmd {
            get { return this._exportMediaSheetCmd; }
        }

        ///<summary>Populate media properties Command</summary>
        public RelayCommand PopulateMediaPropsCmd {
            get { return this._populateMediaPropsCmd; }
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
        ///// <summary>
        ///// Initializes a new instance of the <see cref="ProjectActionsVModel"/> class.
        ///// </summary>
        //public ProjectActionsVModel()
        //    : this("ProjectActionsVModel") {
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectActionsVModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public ProjectActionsVModel(ImageProjectVModel parent, string title="Actions") {
            this.Title = title;
            this.Parent = parent;
            Init();
            this.InitCommands();
        }

        /// <summary>
        /// Initialize business content.
        /// This method can be called more than once
        /// </summary>
        void Init() {
            this.Status = this.Parent.Status;
            this._uiHelper = this.Locator.GetService<IUIHelper>();
            this._log = this.Locator.GetService<ILog>();
            //this.CurrentActions.Add(new ActionStatusVModel("Test 1") { MaxIndex = 100, CurrentIndex = 12 });
            //this.CurrentActions.Add(new ActionStatusVModel("Test 2") { MaxIndex = 100, CurrentIndex = 52 });
        }
        #endregion

        #region Command plumbing

        string _lastSelectedFolder;

        IList<MediaFile> GetSelectedMedia() {
            return this.Parent.MediaItems;
        }

        void DoCopyMediaToFolder(object prm) {
            if (this.UIHelper.SelectFolder("Select target folder to copy media files", ref this._lastSelectedFolder)) {
                List<MediaFile> src = new List<MediaFile>(this.GetSelectedMedia()); // Get the work set
                if (!this.UIHelper.GetUserConfirmation(string.Format("You are going to copy {0} files to {1}\r\n\r\n\t\tContinue?", src.Count, this._lastSelectedFolder))) return;
                ClearCompleted();
                ActionStatusVModel avm = new ActionStatusVModel(string.Format("Copy {0} files to {1}", src.Count, this._lastSelectedFolder)) {
                    MaxIndex = src.Count,
                    MinIndex = 1,
                    CurrentIndex = 0
                };
                this.CurrentActions.Insert(0, avm);
                Task.Factory.StartNew(() => this.CopyMediaToFolder(avm, src, this._lastSelectedFolder));
            }
        }

        void CopyMediaToFolder(ActionStatusVModel avm, IList<MediaFile> source, string targetFolder) {
            string tfn=null, tnm="n/a";
            MediaFile mf;
            StringBuilder err= new StringBuilder();
            int cx = 0;
            Func<string, bool> onError = (em) => {
                avm.SetError(cx + 1, em);
                this.Parent.Status.SetError(em);
                err.AppendLine(em);
                return true;
            };

            for (int i=0; i<source.Count; i++) {
                cx = i;
                mf = source[i];
                tfn = "n/a";
                try {
                    tnm = mf.GetFullPath();
                    tfn = Path.Combine(this._lastSelectedFolder, Path.GetFileName(tnm));
                    CopyAllExtensions(tnm, tfn, onError);
                    //File.Copy(mf.FullName, tfn);
                    avm.SetSuccess(i+1, mf.Title);
                }
                catch (Exception x) {
                    string em= string.Format("Failed to copy file \"{0}\" to \"{1}\". {2}: {3}", mf.FullName, tfn, x.GetType().Name, x.Message);
                    onError(em);
                    //avm.SetError(i+1, em);
                    //this.Parent.Status.SetError(em);
                    ////this.Log.LogTechError(em, x);
                    //err.AppendLine(em);
                }
                //System.Threading.Thread.Sleep(500);
            }
            if (avm.ErrorCount>0) {
                this.Parent.Status.SetError(string.Format("Completed with {1} error(s): {0}\r\n{2}", avm.Title, avm.ErrorCount, err));
            }
            else {
                this.Parent.Status.SetPositive(string.Format("Completed: {0}", avm.Title));
            }
        }

        void DoRenameMedia(object prm) {
            MediaRenameVModel vm = new MediaRenameVModel();
            vm.AddMediaFiles(this.GetSelectedMedia());
            vm.Processor.AddDefinition(new TTSubName());
            vm.Processor.AddDefinition(new TTCounter());
            vm.Processor.AddDefinition(new TTConst());
            vm.Processor.AddDefinition(new TTMarker());
            foreach (var rd in this.Project.RatingDefinitions) {
                vm.Processor.AddDefinition(new TTRating(rd));
            }
            foreach (var cd in this.Project.CategoryDefinitions) {
                vm.Processor.AddDefinition(new TTCategory(cd));
            }
            vm.SetConfiguration(_lastRenameConfig);
            Views.MeidaRenameView vw = new Views.MeidaRenameView();
            vw.Owner = this.UIHelper.GetMainWindow();
            vw.FontSize = vw.Owner.FontSize;
            vw.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            vw.SetViewModel(vm);
            vw.ShowDialog();
            if (vm.IsConfirmed) {
                this._lastRenameConfig = vm.GetConfiguration();
                // Do actual rename 
                if (!this.UIHelper.GetUserConfirmation(string.Format("You are going to rename {0} files using pattern\r\n\r\n\t{1}\r\n\r\nProject will be updated to point to the new files and saved.\r\n\r\n\t\tContinue?",
                    vm.Items.Count, vm.Formula))) return;
                ClearCompleted();
                ActionStatusVModel avm = new ActionStatusVModel(string.Format("Reanme {0} files according to pattern {1}", vm.Items.Count, vm.Formula)) {
                    MaxIndex = vm.Items.Count,
                    MinIndex = 1,
                    CurrentIndex = 0
                };
                string tfn = null, tnm="n/a";
                MediaFile mf;
                StringBuilder err = new StringBuilder();

                this.CurrentActions.Insert(0, avm);
                var curr = this.Parent.CurrentMedia;
                this.Parent.CurrentMedia = null;

                int cx = 0;
                Func<string, bool> onError = (em) => {
                    avm.SetError(cx + 1, em);
                    this.Parent.Status.SetError(em);
                    err.AppendLine(em);
                    return true;
                };


                for (int i = 0; i < vm.Items.Count; i++) {
                    cx = i;
                    mf = vm.Items[i].Key;
                    try {
                        tnm = mf.GetFullPath();
                        tfn = Path.Combine(Path.GetDirectoryName(tnm), vm.Items[i].Value);
                        tfn = tfn + Path.GetExtension(tnm);
                        File.Move(tnm, tfn);
                        mf.FullName = tfn; // If other files fail project still shoud be OK
                        mf.Title = vm.Items[i].Value;
                        MoveAllExtensions(tnm, tfn, onError);
                        avm.SetSuccess(i + 1, mf.Title);
                    }
                    catch (Exception x) {
                        string em = string.Format("Failed to rename file \"{0}\" to \"{1}\". {2}: {3}", tnm, vm.Items[i].Value, x.GetType().Name, x.Message);
                        onError(em);
                        //avm.SetError(i + 1, em);
                        //this.Parent.Status.SetError(em);
                        ////this.Log.LogTechError(em, x);
                        //err.AppendLine(em);
                    }
                }
                if (avm.ErrorCount > 0) {
                    this.Parent.Status.SetError(string.Format("Completed with {1} error(s): {0}\r\n{2}", avm.Title, avm.ErrorCount, err));
                }
                else {
                    this.Parent.Status.SetPositive(string.Format("Completed: {0}", avm.Title));
                }
                this.Parent.CurrentMedia = curr;
                this.Parent.SaveCmd.ExecuteIfCan();
            }
        }

        void DoMoveMediaToFolder(object prm) {
            if (this.UIHelper.SelectFolder("Select target folder to move media files", ref this._lastSelectedFolder)) {
                List<MediaFile> src = new List<MediaFile>(this.GetSelectedMedia()); // Get the work set
                if (!this.UIHelper.GetUserConfirmation(string.Format("You are going to move {0} files to {1}\r\n\r\nProject will be updated to point to the new location and saved.\r\n\r\n\t\tContinue?", src.Count, this._lastSelectedFolder))) return;
                ClearCompleted();
                ActionStatusVModel avm = new ActionStatusVModel(string.Format("Move {0} files to {1}", src.Count, this._lastSelectedFolder)) {
                    MaxIndex = src.Count,
                    MinIndex= 1,
                    CurrentIndex = 0
                };
                this.CurrentActions.Insert(0, avm);
                var curr = this.Parent.CurrentMedia;
                this.Parent.CurrentMedia = null;
                this.MoveMediaToFolder(avm, src, this._lastSelectedFolder);
                this.Parent.CurrentMedia = curr;
                this.Parent.SaveCmd.ExecuteIfCan();
            }
        }

        void MoveMediaToFolder(ActionStatusVModel avm, IList<MediaFile> source, string targetFolder) {
            string tfn = null, tnm;
            MediaFile mf;
            StringBuilder err = new StringBuilder();
            int cx=0;
            Func<string, bool> onError = (em) => {
                avm.SetError(cx + 1, em);
                this.Parent.Status.SetError(em);
                err.AppendLine(em);
                return true;
            };
            for (int i = 0; i < source.Count; i++) {
                mf = source[i];
                tfn = "n/a";
                cx = i;
                try {
                    tnm = mf.GetFullPath();
                    tfn = Path.Combine(this._lastSelectedFolder, Path.GetFileName(tnm));
                    File.Move(tnm, tfn);
                    mf.FullName = tfn; // If other files fail project still shoud be OK
                    MoveAllExtensions(tnm, tfn, onError);
                    avm.SetSuccess(i + 1, mf.Title);
                }
                catch (Exception x) {
                    string em = string.Format("Failed to move file \"{0}\" to \"{1}\". {2}: {3}", mf.FullName, tfn, x.GetType().Name, x.Message);
                    onError(em);
                    //avm.SetError(i + 1, em);
                    //this.Parent.Status.SetError(em);
                    //err.AppendLine(em);
                }
                //System.Threading.Thread.Sleep(500);
            }
            if (avm.ErrorCount > 0) {
                this.Parent.Status.SetError(string.Format("Completed with {1} error(s): {0}\r\n{2}", avm.Title, avm.ErrorCount, err));
            }
            else {
                this.Parent.Status.SetPositive(string.Format("Completed: {0}", avm.Title));
            }
         }

        static IEnumerable<KeyValuePair<string, string>> GetMatchingFiles(string srcFileName, string trgFileName) {
            string folder = Path.GetDirectoryName(srcFileName);
            string pattern = Path.GetFileNameWithoutExtension(srcFileName) + ".*";
            string trgFolder = Path.GetDirectoryName(trgFileName);
            string trgFNm = Path.GetFileNameWithoutExtension(trgFileName);
            string ext, trgPath = Path.Combine(trgFolder, trgFNm);
            foreach (var sfn in Directory.EnumerateFiles(folder, pattern, SearchOption.TopDirectoryOnly)) {
                ext = Path.GetExtension(sfn);
                yield return new KeyValuePair<string, string>(sfn, trgPath+ext);
            }
        }

        /// <summary>
        /// Moves all extensions for the specified <paramref name="srcFileName"/> to the <paramref name="trgFileName"/> with source extensions
        /// </summary>
        /// <param name="srcFileName">Source file path. Extension does not matter.</param>
        /// <param name="trgFileName">Target file path. Extension does not matter.</param>
        /// <param name="onError">Process error message. Return <c>true</c> to continue or <c>false</c> to stop.</param>
        static void MoveAllExtensions(string srcFileName, string trgFileName, Func<string, bool> onError) {
            foreach(var pr in GetMatchingFiles(srcFileName, trgFileName)) {
                try {
                    File.Move(pr.Key, pr.Value);
                }
                catch(Exception x) {
                    if (!onError(x.ToShortMsg(string.Format("{0} file \"{1}\" to \"{2}\"",
                        AreInTheSameFolder(pr.Key, pr.Value) ? "Rename" : "Move",
                        pr.Key, pr.Value))))
                        return;
                }
            }
        }

        static bool AreInTheSameFolder(string filePath1, string filePath2) {
            return Path.GetDirectoryName(filePath1 ?? "").Equals(Path.GetDirectoryName(filePath2 ?? ""), StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Copies all extensions for the specified <paramref name="srcFileName"/> to the <paramref name="trgFileName"/> with source extensions
        /// </summary>
        /// <param name="srcFileName">Source file path. Extension does not matter.</param>
        /// <param name="trgFileName">Target file path. Extension does not matter.</param>
        /// <param name="onError">Process error message. Return <c>true</c> to continue or <c>false</c> to stop.</param>
        static void CopyAllExtensions(string srcFileName, string trgFileName, Func<string, bool> onError) {
            foreach (var pr in GetMatchingFiles(srcFileName, trgFileName)) {
                try {
                    File.Copy(pr.Key, pr.Value);
                }
                catch (Exception x) {
                    if (!onError(x.ToShortMsg(string.Format("Copy file \"{0}\" to \"{1}\"", pr.Key, pr.Value))))
                        return;
                }
            }
        }


        bool HasSelectedMedia(object prm) {
            return true;
            //return this.SetSize > 0;
        }

        /// <summary>
        /// Clears the completed operations from UI.
        /// </summary>
        void ClearCompleted() {
            while (this.CurrentActions.Count > 5) {
                var lavm = this.CurrentActions.LastOrDefault((r) => r.CurrentIndex >= r.MaxIndex);
                if (lavm == null) return;
                this.CurrentActions.Remove(lavm);
            }
        }


        ///<summary>Execute Mark all selected media items Command</summary>
        void DoMarkAllCmd(object prm = null) {
            foreach (var mf in this.GetSelectedMedia()) {
                mf.IsMarked = true;
            }
        }


        ///<summary>Execute Unmark all selected media items Command</summary>
        void DoUnMarkAllCmd(object prm = null) {
            foreach (var mf in this.GetSelectedMedia()) {
                mf.IsMarked = false;
            }
        }

        ///<summary>Execute Inverse mark on selected items Command</summary>
        void DoinverseMarkCmd(object prm = null) {
            foreach (var mf in this.GetSelectedMedia()) {
                mf.IsMarked = !mf.IsMarked;
            }
        }

        ///<summary>Check if Inverse mark on selected items Command can be executed</summary>
        bool CaninverseMarkCmd(object prm = null) {
            return true;
        }


        ///<summary>Execute Remove from project Command</summary>
        void DoExcludeCmd(object prm = null) {
            List<MediaFile> src = new List<MediaFile>(this.GetSelectedMedia()); // Get the work set
            if (!this.UIHelper.GetUserConfirmation(string.Format("You are going to remove {0} files from the project.\r\n"+
                "Actual media files will not be changed.\r\n\r\n\t\tContinue?", 
                src.Count, this._lastSelectedFolder))) return;
            foreach (var mf in src) {
                this.Project.MediaFiles.Remove(mf);
            }
            this.Parent.MediaItems.Clear();
            this.Status.SetPositive(string.Format("{0} file(s) have been removed from the project.\r\nClick Save to finalize.", src.Count));
        }



        ///<summary>Execute Export to Movie Maker Project Command</summary>
        void DoMovMakerProjectCmd(object prm = null) {
            try {
                FileAccessRequest far = new FileAccessRequest();
                far.IsForReading = false;
                far.IsMultiSelect = false;
                far.ExtensionFilter = "Movie Maker project (*.wlmp)|*.wlmp|XML Documents (*.xml)|*.xml|All files (*.*)|*.*";
                far.ExtensionFilterIndex = 1;
                far.SuggestedFileName = "MyProject.wlmp";
                InformationRequest irq = new InformationRequest() {
                    ResultType = typeof(System.IO.File),
                    Prompt = "Save as Movie Maker Project file",
                    Tag = far
                };
                string fileName = null;
                irq.CompleteMethod = (rq) => {
                    if (rq.Result != null) fileName = rq.Result.ToString();
                };
                UIBus uiBus = AppContext.Current.GetServiceViaLocator<UIBus>();
                uiBus.Send(new UIMessage(this, UIBusMessageTypes.InformationRequest, irq));
                if (fileName != null) {
                    MovieMakerHelper.MovieOptions options = new MovieMakerHelper.MovieOptions() {
                        ImgDisplayTime = 7,
                        TargetFileName = fileName,
                        Name = this.Project.Title,
                        MediaFiles = new List<MediaFile>(this.GetSelectedMedia())
                    };
                    MovieMakerHelper mmHelper = new MovieMakerHelper(options);
                    mmHelper.CreateMMakerProject().Save(fileName);
                    this.Status.SetPositive(string.Format("MovieMaker project file {0} has been created.", fileName));
                }
            }
            catch (BizException bx) {
                this.Status.SetError(bx.Message);
            }
            catch (Exception x) {
                this.Status.SetError(string.Format("Faield to save Movie Maker project. {0}: {1}", x.GetType().Name, x.Message), x);
            }
        }

        ///<summary>Execute Export media sheet Command</summary>
        void DoExportMediaSheetCmd(object prm = null) {
            try {
                FileAccessRequest far = new FileAccessRequest();
                far.IsForReading = false;
                far.IsMultiSelect = false;
                far.ExtensionFilter = "XML Documents (*.xml)|*.xml|All files (*.*)|*.*";
                far.ExtensionFilterIndex = 1;
                far.SuggestedFileName = "MyProject.xml";
                InformationRequest irq = new InformationRequest() {
                    ResultType = typeof(System.IO.File),
                    Prompt = "Save as XML Media List file",
                    Tag = far
                };
                string fileName = null;
                List<MediaFile> src = new List<MediaFile>(this.GetSelectedMedia()); // Get the work set
                irq.CompleteMethod = (rq) => {
                    if (rq.Result != null) fileName = rq.Result.ToString();
                };
                UIBus uiBus = AppContext.Current.GetServiceViaLocator<UIBus>();
                uiBus.Send(new UIMessage(this, UIBusMessageTypes.InformationRequest, irq));
                if (fileName != null) {
                    XElement xrt = new XElement("mediaList",
                        new XAttribute(XNames.xaName, this.Project.Title),
                        new XAttribute("timestamp", DateTime.Now));
                    XDocument xdc = new XDocument(new XDeclaration("1.0", Encoding.UTF8.EncodingName, null), xrt);
                    XElement xr;
                    foreach (var mf in src) {
                        if (null != (xr = mf.GetMediaListEntry())) {
                            xrt.Add(xr);
                        }
                    }
                    xdc.Save(fileName);
                    this.Status.SetPositive(string.Format("Media List file {0} has been created.", fileName));
                }
            }
            catch (BizException bx) {
                this.Status.SetError(bx.Message);
            }
            catch (Exception x) {
                this.Status.SetError(x.ToShortMsg("Save Media List"));
            }
        }

        ///<summary>Execute Populate media properties Command</summary>
        void DoPopulateMediaPropsCmd(object prm = null) {
            List<MediaFile> src = new List<MediaFile>(this.GetSelectedMedia()); // Get the work set
            if (!this.UIHelper.GetUserConfirmation(
                string.Format("You are going to load each of {0} files in order to populate media properties.\r\nIt can take some time.\r\n\r\n\t\tContinue?", src.Count))) 
                return;
            ClearCompleted();
            ActionStatusVModel avm = new ActionStatusVModel(string.Format("Populating media properties for {0} files", src.Count)) {
                MaxIndex = src.Count,
                MinIndex = 1,
                CurrentIndex = 0
            };
            this.CurrentActions.Insert(0, avm);
            var curr = this.Parent.CurrentMedia;
            this.Parent.CurrentMedia = null;
            Task.Factory.StartNew(() => this.PopulateMediaMediaProps(avm, src, curr));
        }

        void PopulateMediaMediaProps(ActionStatusVModel avm, IList<MediaFile> source, MediaFile toSetCurrent) {
            string tfn = null;
            MediaFile mf;
            StringBuilder err = new StringBuilder();
            for (int i = 0; i < source.Count; i++) {
                mf = source[i];
                tfn = "n/a";
                try {
                    mf.GetMetadata();
                    avm.SetSuccess(i + 1, mf.Title);
                }
                catch (Exception x) {
                    string em = x.ToShortMsg(mf.FullName);
                    avm.SetError(i + 1, em);
                    this.Parent.Status.SetError(em);
                    //this.Log.LogTechError(em, x);
                    err.AppendLine(em);
                }
                //System.Threading.Thread.Sleep(500);
            }
            if (avm.ErrorCount > 0) {
                this.Parent.Status.SetError(string.Format("Completed with {1} error(s): {0}\r\n{2}. Click Save to finalize.", avm.Title, avm.ErrorCount, err));
            }
            else {
                this.Parent.Status.SetPositive(string.Format("Completed: {0}.\r\nClick Save to finalize.", avm.Title));
            }
            this.RunOnUIThread(()=>this.Parent.CurrentMedia= toSetCurrent);
        }

        ///<summary>Execute Slide show Command</summary>
        void DoSlideShowCmd(object prm = null) {
            List<MediaFile> src = new List<MediaFile>(from mf in this.GetSelectedMedia()
                                                      where mf.MediaType== MediaTypes.Image
                                                      orderby mf.OrderWeight, mf.Title
                                                      select mf); // Get the work set
            if (src.Count>0) {
                CancellationTokenSource cncToken = new CancellationTokenSource();
                ActionStatusVModel avm = new ActionStatusVModel(string.Format("Slide Show for {0} files", src.Count)) {
                    MaxIndex = src.Count,
                    MinIndex = 1,
                    CurrentIndex = 0,
                    ExitHitAction = (a) => cncToken.Cancel()
                };
                var ssw = new SlideShowWorker() { Items = src, ItemDurationMs = 5000, SleepStepMS = 200, StopSignal = cncToken.Token };
                ssw.Show = (mf) => {
                    this.Parent.RunOnUIThread(()=> this.Parent.CurrentMedia = mf);
                    avm.SetSuccess(0, mf.Title);
                };
                this.CurrentActions.Insert(0, avm);
                Task.Factory.StartNew(ssw.DoShow).ContinueWith((t)=> {
                    this.RunOnUIThread(() => {
                        this.Parent.Status.SetPositive("Slide Show canceled");
                        this.CurrentActions.Remove(avm);
                    });
                });
                
            }
        }

        class SlideShowWorker {
            public List<MediaFile> Items { get; set; }
            public Action<MediaFile> Show { get; set; }
            public int SleepStepMS { get; set; }
            public int ItemDurationMs { get; set; }

            public CancellationToken StopSignal { get; set; }
            //public Task DoShow(CancellationToken cancelToken) {

            //}

            public void DoShow() {
                int i = 0;
                int stepMs = (this.SleepStepMS<200) ? 200 : this.SleepStepMS;
                int itemMs = (this.ItemDurationMs < 1000) ? 1000 : this.ItemDurationMs;
                if (stepMs > itemMs)
                    stepMs = itemMs;
                DateTime trgChange;
                while (!StopSignal.IsCancellationRequested) {
                    this.Show(Items[i]);
                    trgChange = DateTime.Now.AddMilliseconds(ItemDurationMs);
                    while(trgChange>DateTime.Now) {
                        Thread.Sleep(SleepStepMS);
                        if (StopSignal.IsCancellationRequested)
                            return;
                    }
                    i++;
                    if (i >= Items.Count) i = 0;
                }
            }
        }

        ///<summary>Check if Slide show Command can be executed</summary>
        bool CanSlideShowCmd(object prm = null) {
            return true;
        }

        /// <summary>
        /// Enumerate all the available commands
        /// </summary>
        IEnumerable<RelayCommand> EnumerateCommands() {
            yield return this.CopyMediaToFolderCmd;
            yield return this.RenameMediaCmd;
            yield return this.MoveMediaCmd;
            yield return this.MarkAllCmd;
            yield return this.UnMarkAllCmd;
            yield return this.InverseMarkCmd;
            yield return this.ExcludeCmd;
            yield return this.MovMakerProjectCmd;
            yield return this.ExportMediaSheetCmd;
            yield return this.PopulateMediaPropsCmd;
            yield return this.SlideShowCmd;
        }

        /// <summary>
        /// Initialize commands infrastructure
        /// </summary>
        void InitCommands() {
            this._copyMediaToFolderCmd = new RelayCommand(UIOperations.Copy, DoCopyMediaToFolder, HasSelectedMedia);
            this._renameMediaCmd = new RelayCommand(UIOperations.Rename, DoRenameMedia, HasSelectedMedia);
            this._moveMediaCmd = new RelayCommand(UIOperations.Move, DoMoveMediaToFolder, HasSelectedMedia);
            this._markAllCmd = new RelayCommand(UIOperations.Select, DoMarkAllCmd, HasSelectedMedia);
            this._unMarkAllCmd = new RelayCommand(UIOperations.Unselect, DoUnMarkAllCmd, HasSelectedMedia);
            this._inverseMarkCmd = new RelayCommand(UIOperations.Inverse, DoinverseMarkCmd, HasSelectedMedia);
            this._excludeCmd = new RelayCommand(UIOperations.Delete, DoExcludeCmd, HasSelectedMedia);
            this._movMakerProjectCmd = new RelayCommand(UIOperations.Export2MovieMaker, DoMovMakerProjectCmd, HasSelectedMedia);
            this._exportMediaSheetCmd = new RelayCommand(UIOperations.ExportMediaSheet, DoExportMediaSheetCmd, HasSelectedMedia);
            this._populateMediaPropsCmd = new RelayCommand(UIOperations.PopulateMediaProps, DoPopulateMediaPropsCmd, HasSelectedMedia);
            this._slideShowCmd = new RelayCommand(UIOperations.SlideShow, DoSlideShowCmd, CanSlideShowCmd);

            ObservableCollection<CommandVModel> cmdVms = new ObservableCollection<CommandVModel>();
            cmdVms.Add(new CommandVModel(MarkAllCmd) { Name = "+ Mark", Description = "Mark all selected media items" });
            cmdVms.Add(new CommandVModel(UnMarkAllCmd) { Name = "- Mark", Description = "Unmark all selected media items" });
            cmdVms.Add(new CommandVModel(InverseMarkCmd) { Name = "~ Mark", Description = "Inverse mark on selected items" });
            cmdVms.Add(new CommandVModel(CopyMediaToFolderCmd) { Name = "Copy", Description = "Copy the selected media files to another folder" });
            cmdVms.Add(new CommandVModel(RenameMediaCmd) { Name = "Rename", Description = "Rename the selected media files by pattern" });
            cmdVms.Add(new CommandVModel(MoveMediaCmd) { Name = "Move", Description = "Move the selected media files to another folder" });
            cmdVms.Add(new CommandVModel(ExcludeCmd) { Name = "Exclude", Description = "Remove media from the project" });
            cmdVms.Add(new CommandVModel(PopulateMediaPropsCmd) { Name = "Load Media Props", Description = "Populate media properties (e.g. duration and bitrate)" });
            cmdVms.Add(new CommandVModel(MovMakerProjectCmd) { Name = ">> Movie Maker", Description = "Export as Movie Maker Project" });
            cmdVms.Add(new CommandVModel(ExportMediaSheetCmd) { Name = ">> Media List", Description = "Export media file information in form of XML with human-friendly data" });
            cmdVms.Add(new CommandVModel(SlideShowCmd) { Name = "SlideShow", Description = "Slide show" });

            AddExternalCommands(cmdVms);

            CommandVModels = cmdVms;
        }

        void AddExternalCommands(ObservableCollection<CommandVModel> cmdVms) {
            ExternalCommand xcmd;
            XCmdHelper xh;
            RelayCommand rcmd;
            foreach (var xci in (from ci in AppContext.Current.EnumerateAppCfgItems((k)=>k.StartsWith("xcmd:")) orderby ci.Code select ci)) {
                try {
                    xcmd = ExternalCommand.CreateByCfg(xci.Value);
                    xh = new XCmdHelper() { Parent = this, XCmd = xcmd };
                    rcmd = new RelayCommand(UIOperations.Undefined, xh.Execute);
                    cmdVms.Add(new CommandVModel(rcmd) { Name = xcmd.Title, Description = xcmd.Description });
                }
                catch (Exception x) {
                    AppContext.Current.LogTechError(x.ToShortMsg("Initialize External command"), x);
                    this.Status.SetError(x.Message);
                }
            }
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

        class XCmdHelper {
            public ExternalCommand XCmd { get; set; }
            public ProjectActionsVModel Parent { get; set; }

            public void Execute(object prm=null) {
                List<MediaFile> src = new List<MediaFile>(Parent.GetSelectedMedia()); // Get the work set
                if (!Parent.UIHelper.GetUserConfirmation(string.Format("You are going to execute external command {1} for {0} files\r\nCommand description:\r\n{2}\r\n\r\n\t\tContinue?", 
                    src.Count, this.XCmd.Title, this.XCmd.Description))) return;
                Parent.ClearCompleted();
                ActionStatusVModel avm = new ActionStatusVModel(string.Format("{1} for {0} files", src.Count, this.XCmd.Title)) {
                    MaxIndex = src.Count,
                    MinIndex = 1,
                    CurrentIndex = 0
                };
                Parent.CurrentActions.Insert(0, avm);
                Task.Factory.StartNew(() => this.Execute(avm, src));

            }

            string GetPrmVal(string key) {
                return this.Parent.Project.VarMap.GetValueSafe(key);
            }

            void Execute(ActionStatusVModel avm, IList<MediaFile> source) {
                string tfn = null;
                MediaFile mf;
                StringBuilder err = new StringBuilder();
                for (int i = 0; i < source.Count; i++) {
                    mf = source[i];
                    if (!XCmd.CanExecute(mf)) {
                        continue;
                    }
                    tfn = "n/a";
                    try {
                        XCmd.Execute(mf, this.GetPrmVal);
                        avm.SetSuccess(i + 1, mf.Title);
                    }
                    catch (Exception x) {
                        string em = string.Format("Failed to process file \"{0}\" to \"{1}\". {2}: {3}", mf.FullName, tfn, x.GetType().Name, x.Message);
                        avm.SetError(i + 1, em);
                        this.Parent.Status.SetError(em);
                        //this.Log.LogTechError(em, x);
                        err.AppendLine(em);
                    }
                    //System.Threading.Thread.Sleep(500);
                }
                if (avm.ErrorCount > 0) {
                    this.Parent.Status.SetError(string.Format("Completed with {1} error(s): {0}\r\n{2}", avm.Title, avm.ErrorCount, err));
                }
                else {
                    this.Parent.Status.SetPositive(string.Format("Completed: {0}", avm.Title));
                }
            }

        }

    }

    
}

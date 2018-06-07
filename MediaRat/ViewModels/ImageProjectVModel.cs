using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml.Linq;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {

    ///<summary>View Model for Photo Media Project</summary>
    ///<remarks>
    ///Image metadata handling: http://www.developerfusion.com/article/84474/reading-writing-and-photo-metadata/
    ///</remarks>
    public class ImageProjectVModel : WorkspaceViewModel {
        ///<summary>Media Project</summary>
        private MediaProject _entity;
        /////<summary>Is preview in a separate dedicated window (otherwise in the same window)</summary>
        //private bool _isPopoutMode;
        ///<summary>Current media file</summary>
        private MediaFile _currentMedia;
        ///<summary>Current filter</summary>
        private SourceFilter _currentSourceFilter;
        ///<summary>Current Rating Definition</summary>
        private RatingDefinition _currentRatingDefinition;
        ///<summary>Media File Information</summary>
        private MediaInfoVModel _mediaInfo;
        ///<summary>Category Definitions Editor View Model</summary>
        private CtgDefinitionsVModel _ctgDefinitions;
        ///<summary>Popout view</summary>
        private IManagedView _popoutView;
        ///<summary>Step scale</summary>
        private StepScale _scale= new StepScale();
        ///<summary>Search Criteria View Model</summary>
        private MediaFileSearchVModel _searchVModel;
        ///<summary>Media files to display</summary>
        private ObservableCollection<MediaFile> _mediaItems;
        ///<summary>Is Search Mode</summary>
        private bool _isSearchMode;
        ///<summary>Project Actions</summary>
        private ProjectActionsVModel _projectActions;
        ///<summary>Image cache</summary>
        private ImageCache _cachedImages;
        ///<summary>Allowed extensions</summary>
        private List<KeyValuePairX<MediaTypes, string>> _allowedExtensions;
        ///<summary>Selected Visual Mode</summary>
        private string _selectedVisualMode;
        ///<summary>Action to get highlighted media items</summary>
        private Func<IList<MediaFile>> _getHighlightedItems;
        ///<summary>List of Prop Elements</summary>
        private IList<PropElement> _propBag;
        ///<summary>XSettings</summary>
        private string _xSettingsTxt;

        ///<summary>XSettings</summary>
        public string XSettingsTxt {
            get { return this._xSettingsTxt; }
            set {
                if (this._xSettingsTxt != value) {
                    this._xSettingsTxt = value;
                    this.FirePropertyChanged(nameof(XSettingsTxt));
                }
            }
        }


        ///<summary>List of Prop Elements</summary>
        public IList<PropElement> PropBag {
            get { return this._propBag; }
            set {
                if (this._propBag != value) {
                    this._propBag = value;
                    this.FirePropertyChanged("PropBag");
                    this.ResetViewState();
                }
            }
        }
        

        ///<summary>Action to get highlighted media items</summary>
        public Func<IList<MediaFile>> GetHighlightedItems {
            get { return this._getHighlightedItems; }
            set { this._getHighlightedItems = value; }
        }
        

        ///<summary>Selected Visual Mode</summary>
        public string SelectedVisualMode {
            get { return this._selectedVisualMode; }
            set {
                if (this._selectedVisualMode != value) {
                    this._selectedVisualMode = value;
                    this.FirePropertyChanged("SelectedVisualMode");
                    this.FirePropertyChanged("IsImageSelected");
                }
            }
        }
  
        ///<summary>Allowed extensions</summary>
        public List<KeyValuePairX<MediaTypes, string>> AllowedExtensions {
            get { return this._allowedExtensions??(this._allowedExtensions= this.GetAllowedExtensions());  }
            //set { this._allowedExtensions = value; }
        }
        

        ///<summary>Image cache</summary>
        public ImageCache CachedImages {
            get { return this._cachedImages; }
            set { this._cachedImages = value; }
        }
        

        ///<summary>Project Actions</summary>
        public ProjectActionsVModel ProjectActions {
            get { return this._projectActions; }
            protected set { this._projectActions = value; }
        }

        ///<summary>Is Search Mode</summary>
        public bool IsSearchMode {
            get { return this._isSearchMode; }
            set {
                if (this._isSearchMode != value) {
                    this._isSearchMode = value;
                    this.FirePropertyChanged("IsSearchMode");
                    this.ResetViewState();
                }
            }
        }        

        ///<summary>Media files to display</summary>
        public ObservableCollection<MediaFile> MediaItems {
            get { return this._mediaItems; }
            set {
                if (this._mediaItems != value) {
                    this._mediaItems = value;
                    this.FirePropertyChanged("MediaItems");
                }
            }
        }
        

        ///<summary>Search Criteria View Model</summary>
        public MediaFileSearchVModel SearchVModel {
            get { return this._searchVModel; }
            //set { this._searchVModel = value; }
        }
        

        ///<summary>Step scale</summary>
        public StepScale Scale {
            get { return this._scale; }
            //set { this._scale = value; }
        }
                


        ///<summary>Category Definitions Editor View Model</summary>
        public CtgDefinitionsVModel CtgDefinitions {
            get { return this._ctgDefinitions; }
            protected set { this._ctgDefinitions = value; }
        }
        

        ///<summary>Media File Information</summary>
        public MediaInfoVModel MediaInfo {
            get { return this._mediaInfo; }
            protected set { this._mediaInfo = value; }
        }
        

        ///<summary>Current Rating Definition</summary>
        public RatingDefinition CurrentRatingDefinition {
            get { return this._currentRatingDefinition; }
            set {
                if (this._currentRatingDefinition != value) {
                    this._currentRatingDefinition = value;
                    this.FirePropertyChanged("CurrentRatingDefinition");
                    this.ResetViewState();
                }
            }
        }
        

        ///<summary>Current filter</summary>
        public SourceFilter CurrentSourceFilter {
            get { return this._currentSourceFilter; }
            set {
                if (this._currentSourceFilter != value) {
                    this._currentSourceFilter = value;
                    this.FirePropertyChanged("CurrentSourceFilter");
                    this.ResetViewState();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether image is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if image is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsImageSelected {
            get { return (this.CurrentMedia!=null) && (this.CurrentMedia.MediaType== MediaTypes.Image); }
        }

        /// <summary>
        /// Gets a value indicating whether video is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if video is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsVideoSelected {
            get { return (this.CurrentMedia != null) && (this.CurrentMedia.MediaType == MediaTypes.Video); }
        }


        ///<summary>Current media file</summary>
        public MediaFile CurrentMedia {
            get { return this._currentMedia; }
            set {
                if (this._currentMedia != value) {
                    var oldMf = this._currentMedia;
                    this._currentMedia = value;
                    this.MediaInfo.Entity = value;
                    this.FirePropertyChanged("CurrentMedia");
                    this.SelectedVisualMode = this.GetSelectedVisualMode();
                    //this.Status.Clear();
                    this.CachedImages.SetMediaFile(value);
                    this.ResetViewState();
                    if (oldMf != null) {
                        oldMf.RefreshUiCue();
                    }
                }
            }
        }
        

        /////<summary>Is preview in a separate dedicated window (otherwise in the same window)</summary>
        //public bool IsPopoutMode {
        //    get { return this._isPopoutMode; }
        //    set {
        //        if (this._isPopoutMode != value) {
        //            this._isPopoutMode = value;
        //            this.FirePropertyChanged("IsPopoutMode");
        //        }
        //    }
        //}
        

        ///<summary>Media Project</summary>
        public MediaProject Entity {
            get { return this._entity; }
            set {
                if (this._entity != value) {
                    this._entity = value;
                    this.CtgDefinitions.Project= 
                        this.MediaInfo.Project = 
                        this.SearchVModel.Project= this.Entity;
                    this.FirePropertyChanged("Entity");
                    this.ResetViewState();
                    this.UpdateMediaItems();
                    this.XSettingsTxt = (value == null) ? string.Empty : value.XSettings.ToString();
                }
            }
        }

        ///<summary>Popout view</summary>
        public IManagedView PopoutView {
            get { return this._popoutView; }
            set {
                if (this._popoutView != value) {
                    this._popoutView = value;
                    this.FirePropertyChanged("PopoutView");
                    this.SelectedVisualMode = this.GetSelectedVisualMode();
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
        ///<summary>Populate Media files Command</summary>
        private RelayCommand _populateMediaCmd;
        ///<summary>Add search filter Command</summary>
        private RelayCommand _addSourceFilterCmd;
        ///<summary>Delete Search filter Command</summary>
        private RelayCommand _deleteSourceFilterCmd;
        ///<summary>Add subfolders as a sorce filters Command</summary>
        private RelayCommand _addSourceFilterSubTreeCmd;
        ///<summary>Add Rating defintion Command</summary>
        private RelayCommand _addRatingCmd;
        ///<summary>Delete Rating Definition Command</summary>
        private RelayCommand _deleteRatingCmd;
        ///<summary>Popout Command</summary>
        private RelayCommand _popoutCmd;
        ///<summary>Search Command</summary>
        private RelayCommand _searchCmd;
        ///<summary>Move to the Next Command</summary>
        private RelayCommand _goNextCmd;
        ///<summary>Move to the Previous Command</summary>
        private RelayCommand _goPreviousCmd;
        ///<summary>Release filter Command</summary>
        private RelayCommand _releaseFilterCmd;
        ///<summary>Find Next matching Command</summary>
        private RelayCommand _findNextCmd;
        ///<summary>Get Prop Elements from the current item Command</summary>
        private RelayCommand _getPropElemsCmd;
        ///<summary>Apply Prop Elements Command</summary>
        private RelayCommand _applyPropElemsCmd;
        ///<summary>Adjust Order Weights Command keeps order by weight but change weigh value</summary>
	    private RelayCommand _adjustOrderWeightsCmd;
        ///<summary>Set Order Weights Command</summary>
	    private RelayCommand _setOrderWeightsCmd;
        ///<summary>Open media in default OS Command</summary>
	    private RelayCommand _openMediaCmd;
        ///<summary>Apply XSettings Command</summary>
        private RelayCommand _applyXSettingsCmd;

        ///<summary>Apply XSettings Command</summary>
        public RelayCommand ApplyXSettingsCmd {
            get { return this._applyXSettingsCmd; }
        }

        ///<summary>Open media in default OS Command</summary>
        public RelayCommand OpenMediaCmd {
            get { return this._openMediaCmd; }
        }

        ///<summary>Set Order Weights Command</summary>
        public RelayCommand SetOrderWeightsCmd {
            get { return this._setOrderWeightsCmd; }
        }

        ///<summary>Adjust Order Weights Command keeps order by weight but change weigh value</summary>
        public RelayCommand AdjustOrderWeightsCmd {
            get { return this._adjustOrderWeightsCmd; }
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

        ///<summary>Save Command</summary>
        public RelayCommand SaveCmd {
            get { return this._saveCmd; }
        }

        ///<summary>Save As Command</summary>
        public RelayCommand SaveAsCmd {
            get { return this._saveAsCmd; }
        }

        ///<summary>Delete Search filter Command</summary>
        public RelayCommand DeleteSourceFilterCmd {
            get { return this._deleteSourceFilterCmd; }
        }

        ///<summary>Add search filter Command</summary>
        public RelayCommand AddSourceFilterCmd {
            get { return this._addSourceFilterCmd; }
        }

        ///<summary>Add subfolders as a sorce filters Command</summary>
        public RelayCommand AddSourceFilterSubTreeCmd {
            get { return this._addSourceFilterSubTreeCmd; }
        }


        ///<summary>Populate Media files Command</summary>
        public RelayCommand PopulateMediaCmd {
            get { return this._populateMediaCmd; }
        }

        ///<summary>Add Rating defintion Command</summary>
        public RelayCommand AddRatingCmd {
            get { return this._addRatingCmd; }
        }
        ///<summary>Delete Rating Definition Command</summary>
        public RelayCommand DeleteRatingCmd {
            get { return this._deleteRatingCmd; }
        }

        ///<summary>Popout Command</summary>
        public RelayCommand PopoutCmd {
            get { return this._popoutCmd; }
        }

        ///<summary>Get Prop Elements from the current item Command</summary>
        public RelayCommand GetPropElemsCmd {
            get { return this._getPropElemsCmd; }
        }

        ///<summary>Apply Prop Elements Command</summary>
        public RelayCommand ApplyPropElemsCmd {
            get { return this._applyPropElemsCmd; }
        }


        ///<summary>Search Command</summary>
        public RelayCommand SearchCmd {
            get { return this._searchCmd; }
        }

        ///<summary>Find Next matching Command</summary>
        public RelayCommand FindNextCmd {
            get { return this._findNextCmd; }
        }
        
        ///<summary>Release filter Command</summary>
        public RelayCommand ReleaseFilterCmd {
            get { return this._releaseFilterCmd; }
        }


        ///<summary>Move to the Previous Command</summary>
        public RelayCommand GoPreviousCmd {
            get { return this._goPreviousCmd; }
        }


        ///<summary>Move to the Next Command</summary>
        public RelayCommand GoNextCmd {
            get { return this._goNextCmd; }
        }

        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageProjectVModel"/> class.
        /// </summary>
        public ImageProjectVModel()
            : this("Photo Project") {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageProjectVModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public ImageProjectVModel(string title) {
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
            this.Scale.Init(30, 1, 1.15);
            this.MediaInfo = new MediaInfoVModel() { Project = this.Entity };
            this.CtgDefinitions = new CtgDefinitionsVModel() { Project = this.Entity };
            this._searchVModel = new MediaFileSearchVModel() { Project = this.Entity };
            this._projectActions = new ProjectActionsVModel(this);
            this._cachedImages = new ImageCache() { Status = this.Status };
        }

         #endregion

        #region Command plumbing
        void DoExit(object prm) {
            if (this.Locator.GetService<IUIHelper>().GetUserConfirmation("You are closing the project. Probably some information has not be saved.\r\n\r\n\tContinue?")) {
                if (this.PopoutView != null)
                    this.PopoutView.Close();
                this.OnRequestClose();
            }
        }

        void SaveProject(MediaProject src, string targetPath, string password = null) {
            this.Status.Clear();
            try {
                XElement xel = src.GetXml(password);
                XDocument xd = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), xel);
                xd.Save(targetPath);
                src.ProjectFileName = targetPath;
             }
            catch (Exception x) {
                throw new BizException(string.Format("Failed to save project to {0}. {1}: {2}", targetPath, x.GetType().Name, x.Message));
            }
        }

        /// <summary>
        /// Loads the project.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public bool LoadProject(string fileName) {
            try {
                XDocument xd = XDocument.Load(fileName);
                MediaProject mprj = new MediaProject();
                mprj.ApplyConfiguration(xd.Root, null);
                mprj.ProjectFileName = fileName;
                this.Entity= mprj;
                return true;
            }
            catch (Exception x) {
                this.Status.SetError(string.Format("Failed to load {0}. {1}: {2}", fileName, x.GetType().Name, x.Message), x);
            }
            return false;
        }

        void UpdateEntity() {
            this.CtgDefinitions.UpdateEntity();
            this.MediaInfo.UpdateEntity();
            this.Title = string.IsNullOrEmpty(this.Entity.Title) ? "Photo Project" : this.Entity.Title;
            this.MediaInfo.ResetDefinitions();
            this.SearchVModel.ResetDefinitions();
        }

        void DoSave(object p) {
            this.UpdateEntity();
            if (string.IsNullOrEmpty(this.Entity.ProjectFileName)) {
                DoSaveAs(p);
            }
            else {
                try {
                    SaveProject(this.Entity, this.Entity.ProjectFileName, null);
                }
                catch (BizException bx) {
                    this.Status.SetError(bx.Message);
                }
                catch (Exception x) {
                    this.Status.SetError(string.Format("Faield to save project. {0}: {1}", x.GetType().Name, x.Message), x);
                }
            }
        }

        void DoSaveAs(object p) {
            this.UpdateEntity();
            try {
                FileAccessRequest far = new FileAccessRequest();
                far.IsForReading = false;
                far.IsMultiSelect = false;
                far.ExtensionFilter = "Media Rat project (*.xmr)|*.xmr|XML Documents (*.xml)|*.xml|All files (*.*)|*.*";
                far.ExtensionFilterIndex = 1;
                far.SuggestedFileName = "MyProject.xmr";
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
                    SaveProject(this.Entity, fileName, null);
                }
            }
            catch (BizException bx) {
                this.Status.SetError(bx.Message);
            }
            catch (Exception x) {
                this.Status.SetError(string.Format("Faield to save project. {0}: {1}", x.GetType().Name, x.Message), x);
            }
        }

        bool CanDoSave(object p) {
            return true;
        }

        void DoPopulateMedia(object p) {
            this.Status.SetPositive("Populating media files...");
            this.ExecuteAndReport(() => PopulateMediaFiles());
        }

        static string UnQuoute(string source, string start="\"", string end=null) {
            if (string.IsNullOrEmpty(source)) 
                return source;
            if (source.StartsWith(start)) {
                if (end == null)
                    end = start; // The same symbol is used for end
                if (source.EndsWith(end)) {
                    int nwLen = source.Length - start.Length - end.Length;
                    if (nwLen <= 0)
                        return string.Empty;
                    return source.Substring(start.Length, nwLen);
                }
            }
            // At least one of the start|stop markers is missed
            return source;
        }

        void PopulateMediaFiles(object p=null) {
            int nPrj=0, nLocated = 0, nAdded = 0;
            List<SourceFilter> filters = new List<SourceFilter>(this.Entity.SourceFilters);
            Dictionary<string, MediaFile> existing = new Dictionary<string, MediaFile>();
            foreach (var mf in this.Entity.MediaFiles) {
                mf.IsLocated = false;
                existing[mf.FullName.ToLower()] = mf;
            }
            nPrj = existing.Count;
            MediaFile tmf;
            string mfk, sfFolder;
            MediaTypes fileType;
            foreach (var sf in filters) {
                sfFolder = (UnQuoute(sf.Folder)??string.Empty).Trim();
                this.Status.SetPositive(string.Format("Scanning: {0} [{1}]", sf.Folder, sf.Filter));
                foreach (var mf in System.IO.Directory.EnumerateFiles(sfFolder, sf.Filter)) {
                    nLocated++;
                    mfk= mf.ToLower();
                    if (existing.TryGetValue(mfk, out tmf)) {
                        tmf.IsLocated = true;
                    }
                    else {
                        fileType = this.GetFileMediaType(mf);
                        tmf = this.Entity.CreateMediaFile(fileType);
                        if (tmf != null) {
                            tmf.FullName = mfk;
                            tmf.IsLocated = true;
                            tmf.Title = System.IO.Path.GetFileNameWithoutExtension(mf);
                            existing[mfk] = tmf;
                            this.Entity.MediaFiles.Add(tmf);
                            nAdded++;
                        }
                    }
                }
            }
            this.Status.SetPositive(string.Format("Initial state: {0} files, located {1} files ({2} new). End state: {3} files", nPrj, nLocated, nAdded, existing.Count));
        }

        //Wrong idea. Need mapping between MediaTypes and Extensions
        List<KeyValuePairX<MediaTypes, string>> GetAllowedExtensions(MediaTypes mediaType= MediaTypes.Undefined) {
            string cfgSrc;
            List<KeyValuePairX<MediaTypes, string>> rz = new List<KeyValuePairX<MediaTypes, string>>();
            if (mediaType== MediaTypes.Undefined) { // Need all
                foreach (MediaTypes mt in Enum.GetValues(typeof(MediaTypes))) {
                    if (mt != MediaTypes.Undefined) {
                        rz.AddRange(GetAllowedExtensions(mt));
                    }
                }
            }
            else {
                cfgSrc= AppContext.Current.GetAppCfgItem(string.Format("{0}Extensions", mediaType));
                if (cfgSrc != null) {
                    string[] items = cfgSrc.ToLower().Split(new char[] { '|', ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var it in items) {
                        rz.Add(new KeyValuePairX<MediaTypes, string>() { Key = mediaType, Value = "." + it });
                    }
                }
            }
            return rz;
        }


        KeyValuePairX<MediaTypes, string> _lastFound;

        MediaTypes GetFileMediaType(string fullPath) {
            string ext= System.IO.Path.GetExtension(fullPath);
            if (this._lastFound != null) {
                if (string.Compare(_lastFound.Value, ext, true) == 0) {
                    return _lastFound.Key;
                }
            }
            var tm = this.AllowedExtensions.FirstOrDefault((r) => string.Compare(r.Value, ext, true) == 0);
            if (tm == null) {
                return MediaTypes.Undefined;
            }
            else {
                this._lastFound = tm;
                return tm.Key;
            }
        }

        /// <summary>
        /// Processes the dropped files.
        /// </summary>
        /// <param name="files">The files.</param>
        public void ProcessDroppedFiles(IEnumerable<string> files) {
            if (files != null) {
                try {
                    int nPrj = 0, nLocated = 0, nAdded = 0, nSkipped = 0;
                    string mfk;
                    // Need mapping between Media Types and extensions to set MediaFile type by file extension
                    Dictionary<string, MediaFile> existing = new Dictionary<string, MediaFile>();
                    foreach (var mf in this.Entity.MediaFiles) {
                        mf.IsLocated = false;
                        existing[mf.FullName.ToLower()] = mf;
                    }
                    nPrj = existing.Count;
                    MediaFile tmf;
                    MediaTypes fileType;
                    StringBuilder sbErr = new StringBuilder();
                    sbErr.AppendLine();

                    foreach (var f in files) {
                        nLocated++;
                        mfk = f.ToLower();
                        if (existing.TryGetValue(mfk, out tmf)) {
                            tmf.IsLocated = true;
                        }
                        else {
                            fileType = this.GetFileMediaType(f);
                            tmf = this.Entity.CreateMediaFile(fileType);
                            if (tmf != null) {
                                tmf.FullName = mfk;
                                tmf.Title = System.IO.Path.GetFileNameWithoutExtension(f);
                                tmf.IsLocated = true;
                                existing[mfk] = tmf;
                                this.Entity.MediaFiles.Add(tmf);
                                nAdded++;
                            }
                            else {
                                nSkipped++;
                                sbErr.AppendFormat("Unsupported file type: {0}", System.IO.Path.GetFileName(f)).AppendLine();
                            }
                        }
                    }
                    string msg = string.Format("Initial state: {0} files, dropped {1} files ({2} new, {4} skipped). End state: {3} files.{5}",
                        nPrj, nLocated, nAdded, existing.Count, nSkipped, sbErr);
                    if (nSkipped > 0)
                        this.Status.SetError(msg);
                    else
                        this.Status.SetPositive(msg);
                }
                catch (Exception x) {
                    this.Status.SetError(string.Format("Failed to add files. {0}: {1}", x.GetType().Name, x.Message));
                }
            }
        }

        bool CanDoPopulateMedia(object p) {
            return true;
        }

        /// <summary>
        /// The last added folder is used to start search from the same place next time
        /// </summary>
        private string _lastAddedFolder;

        void DoAddSourceFilter(object p) {
            var uiH = Locator.GetService<IUIHelper>();
            if (uiH.SelectFolder("Add source folder", ref this._lastAddedFolder)) {
                this.Entity.SourceFilters.Add(new SourceFilter() { Folder = this._lastAddedFolder, Filter = "*.jpg" });
                this.Status.SetPositive("Click '+Tree' to add all subfolders or Populate to add the matching files from the source folders.");
            }
        }

        bool CanDoAddSourceFilter(object p) {
            return true;
        }

        ///<summary>Execute Add subfolders as a sorce filters Command</summary>
        void DoAddSourceFilterSubTreeCmd(object prm = null) {
            string root = "N/A";
            try {
                root = this.CurrentSourceFilter.Folder;
                string flt = this.CurrentSourceFilter.Filter;
                string newFlt;
                HashSet<string> existing = new HashSet<string>();
                foreach (var sf in this.Entity.SourceFilters) {
                    existing.Add(System.IO.Path.Combine(sf.Folder, sf.Filter).ToLower());
                }
                //System.Diagnostics.Debug.WriteLine("---- For the current filter [{0}] selected root [{1}] subfolders:", this.CurrentSourceFilter.Folder, root);
                foreach (var sd in System.IO.Directory.EnumerateDirectories(root, "*", System.IO.SearchOption.AllDirectories)) {
                    newFlt = System.IO.Path.Combine(sd, flt);
                    if (!existing.Contains(newFlt.ToLower())) {
                        this.Entity.SourceFilters.Add(new SourceFilter() { Folder = sd, Filter= flt });
                    }
                    //System.Diagnostics.Debug.WriteLine(newFlt);
                }

            }
            catch (Exception x) {
                this.Status.SetError(string.Format("Failed to add subfolders for {0}. {1}: {2}", root, x.GetType().Name, x.Message), x);
            }
        }

        ///<summary>Check if Add subfolders as a sorce filters Command can be executed</summary>
        bool CanAddSourceFilterSubTreeCmd(object prm = null) {
            return (this.CurrentSourceFilter != null) && !string.IsNullOrEmpty(this.CurrentSourceFilter.Folder);
        }

        void DoDelSourceFilter(object p) {
            this.Entity.SourceFilters.Remove(this.CurrentSourceFilter);
        }

        bool CanDoDelSourceFilter(object p) {
            return this.CurrentSourceFilter!=null;
        }

        ///<summary>Execute Add Rating defintion Command</summary>
        void DoAddRatingCmd(object prm = null) {
            RatingDefinition rd;
            this.Entity.RatingDefinitions.Add(rd= new RatingDefinition() {
                Marker = "rt1",
                Title = "Rating #1"
            });
            this.CurrentRatingDefinition = rd;
        }

        ///<summary>Check if Add Rating defintion Command can be executed</summary>
        bool CanAddRatingCmd(object prm = null) {
            return true;
        }

        ///<summary>Execute Delete Rating Definition Command</summary>
        void DoDeleteRatingCmd(object prm = null) {
            var rd = this.CurrentRatingDefinition;
            this.Entity.RatingDefinitions.Remove(rd);
            int ix;
            foreach (var mf in this.Entity.MediaFiles) {
                ix= mf.Ratings.FirstIndex((rv)=>rv.Key==rd);
                if (ix >= 0)
                    mf.Ratings.RemoveAt(ix);
            }
        }

        ///<summary>Check if Delete Rating Definition Command can be executed</summary>
        bool CanDeleteRatingCmd(object prm = null) {
            return this.CurrentRatingDefinition!=null;
        }

        ///<summary>Execute Popout Command</summary>
        void DoPopoutCmd(object prm = null) {
            if (this.PopoutView == null) {
                Views.PopoutImage vw = new Views.PopoutImage();
                vw.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                vw.SetViewModel(this);
                this.PopoutView = vw;
                vw.Show();
            }
            else {
                this.PopoutView.EnsureVisible();
            }
        }

        ///<summary>Check if Popout Command can be executed</summary>
        bool CanPopoutCmd(object prm = null) {
            return true;
        }

        ///<summary>Execute Search Command</summary>
        void DoSearchCmd(object prm = null) {
            this.IsSearchMode = true;
            if (this.SearchVModel.Criteria.IsHighlighted.HasValue) {
                if (this.GetHighlightedItems == null) {
                    this.Status.SetError("GetHighlighted delegate is not bound to ViewModel");
                    return;
                }
                this.SearchVModel.Criteria.HighlightedItems.Clear();
                foreach (var mf in this.GetHighlightedItems())
                    this.SearchVModel.Criteria.HighlightedItems.Add(mf);
            }
            this.UpdateMediaItems();
            if (this.MediaItems.Count > 0)
                this.CurrentMedia = this.MediaItems[0];
            else
                this.CurrentMedia = null;
        }

        ///<summary>Check if Search Command can be executed</summary>
        bool CanSearchCmd(object prm = null) {
            return true;
        }

        ///<summary>Execute Search Command</summary>
        void DoReleaseFilterCmd(object prm = null) {
            this.IsSearchMode = false;
            this.SearchVModel.Criteria.HighlightedItems.Clear();
            var slc = this.CurrentMedia;
            this.UpdateMediaItems();
            this.CurrentMedia = slc;
        }

        ///<summary>Check if Search Command can be executed</summary>
        bool CanReleaseFilterCmd(object prm = null) {
            return this.IsSearchMode;
        }

        ///<summary>Execute Search Command</summary>
        void DoFindNextCmd(object prm = null) {
            if ((this.MediaItems != null) && (this.MediaItems.Count > 0)) {
                int ix= 0;
                if (this.CurrentMedia != null) {
                    ix = this.MediaItems.IndexOf(this.CurrentMedia) + 1;
                }
                if (ix < this.MediaItems.Count) {
                    for (int i = ix; i < this.MediaItems.Count; i++) {
                        if (this.SearchVModel.Criteria.IsMatching(this.MediaItems[i])) {
                            this.CurrentMedia = this.MediaItems[i];
                            return;
                        }
                    }
                }
            }
        }

        ///<summary>Check if Search Command can be executed</summary>
        bool CanFindNextCmd(object prm = null) {
            return true;
        }

        ///<summary>Move to the next item Command</summary>
        void DoGoNextCmd(object prm = null) {
            if ((this.MediaItems != null) && (this.MediaItems.Count > 0)) {
                int ix = 0;
                if (this.CurrentMedia != null) {
                    ix = this.MediaItems.IndexOf(this.CurrentMedia) + 1;
                }
                if (ix < this.MediaItems.Count) {
                    this.CurrentMedia = this.MediaItems[ix];
                }
            }
        }

        ///<summary>Check if Move to the next item Command can be executed</summary>
        bool CanGoNextCmd(object prm = null) {
            return true;
        }

        ///<summary>Move to the previous item Command</summary>
        void DoGoPreviousCmd(object prm = null) {
            if ((this.MediaItems != null) && (this.MediaItems.Count > 0)) {
                int ix = 0;
                if (this.CurrentMedia != null) {
                    ix = this.MediaItems.IndexOf(this.CurrentMedia);
                    ix= (ix>0) ? ix-- : 0;

                }
                this.CurrentMedia = this.MediaItems[ix];
            }
        }

        ///<summary>Check if Move to the next item Command can be executed</summary>
        bool CanGoPreviousCmd(object prm = null) {
            return true;
        }

        ///<summary>Execute Get Prop Elements from the current item Command</summary>
        void DoGetPropElemsCmd(object prm = null) {
            this.MediaInfo.UpdateEntity();
            this.PropBag = new List<PropElement>(this.CurrentMedia.EnumeratePropElements());
        }

        ///<summary>Check if Get Prop Elements from the current item Command can be executed</summary>
        bool CanGetPropElemsCmd(object prm = null) {
            return this.CurrentMedia != null;
        }

        ///<summary>Execute Apply Prop Elements Command</summary>
        void DoApplyPropElemsCmd(object prm = null) {
            var vm = new PropElementListVModel();
            vm.Entities = new ObservableCollection<PropElement>(this.PropBag);
            vm.Applicator = (pel) => {
                List<PropElement> actPel= new List<PropElement>(from pe in pel where pe.IsMarked select pe);
                foreach (var mf in this.GetHighlightedItems()) {
                    mf.ApplyPropElements(actPel);
                }
                this.MediaInfo.UpdateView(); // MediaFile got updated so reflect it on View
            };

            var bus = this.Locator.GetService<UIBus>();
            bus.Send(new ApplicationMessage<UIBusMessageTypes>(this, UIBusMessageTypes.InformationRequest,
                new InformationRequest() {
                    Tag = vm,
                    CompleteMethod = (r) => { }
                }));
        }

        ///<summary>Check if Apply Prop Elements Command can be executed</summary>
        bool CanApplyPropElemsCmd(object prm = null) {
            return (this.PropBag!=null) && (this.CurrentMedia!=null);
        }


        ///<summary>Execute Adjust Order Weights Command keeps order by weight but change weigh value</summary>
        void DoAdjustOrderWeightsCmd(object prm = null) {
            this.Entity.AutoAssignOrder<int>((mf) => mf.OrderWeight);
        }

        ///<summary>Check if Adjust Order Weights Command keeps order by weight but change weigh value can be executed</summary>
        bool CanAdjustOrderWeightsCmd(object prm = null) {
            return true;
        }

        ///<summary>Execute Set Order Weights Command</summary>
        void DoSetOrderWeightsCmd(object prm = null) {
            this.Entity.AutoAssignOrder();
        }

        ///<summary>Check if Set Order Weights Command can be executed</summary>
        bool CanSetOrderWeightsCmd(object prm = null) {
            return true;
        }

        ///<summary>Execute Open media in default OS Command</summary>
        void DoOpenMediaCmd(object prm = null) {
            MediaFile mf = (prm as MediaFile) ?? this.CurrentMedia;
            if (mf!=null)
                System.Diagnostics.Process.Start(this.CurrentMedia.GetFullPath());
        }

        ///<summary>Check if Open media in default OS Command can be executed</summary>
        bool CanOpenMediaCmd(object prm = null) {
            MediaFile mf = (prm as MediaFile) ?? this.CurrentMedia;
            return mf!=null;
        }


        ///<summary>Execute Apply XSettings Command</summary>
        void DoAapplyXSettingsCmd(object prm = null) {
            this.Status.Clear();
            try {
                this.Entity.ApplyXSetings(this.XSettingsTxt);
            }
            catch (Exception x) {
                this.ReportError(x);
            }
        }

        ///<summary>Check if Apply XSettings Command can be executed</summary>
        bool CanApplyXSettingsCmd(object prm = null) {
            return true;
        }


        /// <summary>
        /// Enumerate all the available commands
        /// </summary>
        IEnumerable<RelayCommand> EnumerateCommands() {
            yield return this.SaveCmd;
            yield return this.SaveAsCmd;
            yield return this.PopulateMediaCmd;
            yield return this.AddSourceFilterCmd;
            yield return this.AddSourceFilterSubTreeCmd;
            yield return this.DeleteSourceFilterCmd;
            yield return this.AddRatingCmd;
            yield return this.DeleteRatingCmd;
            yield return this.PopoutCmd;
            yield return this.GetPropElemsCmd;
            yield return this.ApplyPropElemsCmd;
            yield return this.SearchCmd;
            yield return this.FindNextCmd;
            yield return this.ReleaseFilterCmd;
            yield return this.GoNextCmd;
            yield return this.GoPreviousCmd;
            yield return this.SetOrderWeightsCmd;
            yield return this.AdjustOrderWeightsCmd;
            yield return this.OpenMediaCmd;
            yield return this.ApplyXSettingsCmd;
            yield return this.ExitCommand;
        }

        /// <summary>
        /// Initialize commands infrastructure
        /// </summary>
        void InitCommands() {
            this._saveCmd = new RelayCommand(UIOperations.Save, DoSave, CanDoSave);
            this._saveAsCmd = new RelayCommand(UIOperations.SaveAs, DoSaveAs, CanDoSave);
            this._populateMediaCmd = new RelayCommand(UIOperations.Search, DoPopulateMedia, CanDoPopulateMedia);
            this._addSourceFilterCmd = new RelayCommand(UIOperations.AddSourceFilter, DoAddSourceFilter, CanDoAddSourceFilter);
            this._addSourceFilterSubTreeCmd = new RelayCommand(UIOperations.AddSourceFilterSubTree, DoAddSourceFilterSubTreeCmd, CanAddSourceFilterSubTreeCmd);
            this._deleteSourceFilterCmd = new RelayCommand(UIOperations.DeleteSourceFilter, DoDelSourceFilter, CanDoDelSourceFilter);
            this._addRatingCmd = new RelayCommand(UIOperations.AddRating, DoAddRatingCmd, CanAddRatingCmd);
            this._deleteRatingCmd = new RelayCommand(UIOperations.DeleteRating, DoDeleteRatingCmd, CanDeleteRatingCmd);
            this._popoutCmd = new RelayCommand(UIOperations.Popout, DoPopoutCmd, CanPopoutCmd);
            this._getPropElemsCmd = new RelayCommand(UIOperations.GetPropElements, DoGetPropElemsCmd, CanGetPropElemsCmd);
            this._applyPropElemsCmd = new RelayCommand(UIOperations.ApplyPropElements, DoApplyPropElemsCmd, CanApplyPropElemsCmd);
            this._searchCmd = new RelayCommand(UIOperations.Search, DoSearchCmd, CanSearchCmd);
            this._releaseFilterCmd = new RelayCommand(UIOperations.ReleaseFilter, DoReleaseFilterCmd, CanReleaseFilterCmd);
            this._findNextCmd = new RelayCommand(UIOperations.FindNext, DoFindNextCmd, CanFindNextCmd);
            this._goNextCmd = new RelayCommand(UIOperations.MoveNext, DoGoNextCmd, CanGoNextCmd);
            this._goPreviousCmd = new RelayCommand(UIOperations.MovePrevious, DoGoPreviousCmd, CanGoPreviousCmd);
            this._setOrderWeightsCmd = new RelayCommand(UIOperations.OrderWeightSet, DoSetOrderWeightsCmd, CanSetOrderWeightsCmd);
            this._adjustOrderWeightsCmd = new RelayCommand(UIOperations.AdjustOrderWeights, DoAdjustOrderWeightsCmd, CanAdjustOrderWeightsCmd);
            this._openMediaCmd = new RelayCommand(UIOperations.Open, DoOpenMediaCmd, CanOpenMediaCmd);
            this._applyXSettingsCmd = new RelayCommand(UIOperations.ApplyXSettingsCmd, DoAapplyXSettingsCmd, CanApplyXSettingsCmd);
            this._exitCmd = new RelayCommand(UIOperations.Exit, DoExit, (p) => true);

            this.SearchVModel.SearchCmd = this.SearchCmd;
            this.SearchVModel.ReleaseCmd = this.ReleaseFilterCmd;
            this.SearchVModel.NextCmd = this.FindNextCmd;

            ObservableCollection<CommandVModel> cmdVms = new ObservableCollection<CommandVModel>();
            cmdVms.Add(new CommandVModel(PopoutCmd) { Name = "Pop out", Description = "Display media in separate window" });
            cmdVms.Add(new CommandVModel(SearchCmd) { Name = "Search", Description = "Search in the project" });
            cmdVms.Add(new CommandVModel(SaveCmd) { Name = "Save", Description = "Save project" });
            cmdVms.Add(new CommandVModel(SaveAsCmd) { Name = "Save as", Description = "Save project with specific file name" });
            cmdVms.Add(new CommandVModel(PopulateMediaCmd) { Name = "Populate", Description = "Populate media files to the project according to source criteria project" });
            cmdVms.Add(new CommandVModel(GetPropElemsCmd) { Name = "Copy Ratings", Description = "Copy raitings and categories of the current media file to apply them later" });
            cmdVms.Add(new CommandVModel(ApplyPropElemsCmd) { Name = "Paste Ratings", Description = "Apply the previously copied raitings and categories to the selected medifiles" });
            cmdVms.Add(new CommandVModel(OpenMediaCmd) { Name = "Launch", Description = "Open media in default OS Command" });
            cmdVms.Add(new CommandVModel(SetOrderWeightsCmd) { Name = "Set Weights", Description = "Set Order Weights by name" });
            cmdVms.Add(new CommandVModel(AdjustOrderWeightsCmd) { Name = "Adjust Weights", Description = "Keep order by weight but change weigh value" });
            cmdVms.Add(new CommandVModel(ExitCommand));
            // cmdVms.Add(new CommandVModel(applyXSettingsCmd) { Name = "applyXSettingsCmd", Description = "Apply XSettings Command" });
            //cmdVms.Add(new CommandVModel(ClonetCmd) { Name = "Clone", Description = "Clone workspace" });
            CommandVModels = cmdVms;
        }

        /// <summary>
        /// Reset presentation attributes according to the current state
        /// </summary>
        void ResetViewState() {
            foreach (var cmd in this.EnumerateCommands()) cmd.Reset(null);
            this.Title = string.IsNullOrEmpty(this.Entity.Title) ? "Photo Project" : this.Entity.Title;
        }

        void UpdateMediaItems() {
            if (this.Entity == null) {
                this.MediaItems = null;
                this.Status.Clear();
                return;
            }
            if (this.IsSearchMode) {
                this.SearchVModel.UpdateCriteria();
                var filter = this.SearchVModel.Criteria.GetFilter();
                this.MediaItems = new ObservableCollection<MediaFile>(
                    from mf in this.Entity.MediaFiles where filter.IsMatching(mf) select mf
                    );
                this.Status.SetPositive(string.Format("{0} out of {1} selected", this.MediaItems.Count, this.Entity.MediaFiles.Count));
            }
            else {
                this.MediaItems = this.Entity.MediaFiles;
                this.Status.SetPositive(string.Format("{0} media file(s)", this.MediaItems.Count));
            }
        }

        string GetSelectedVisualMode() {
            if (this.CurrentMedia == null) return string.Empty;
            switch (this.CurrentMedia.MediaType) {
                case MediaTypes.Image: return (this.PopoutView == null) ? "ImageIn" : "ImageOut";
                case MediaTypes.Video: return (this.PopoutView == null) ? "VideoIn" : "VideoOut";
                case MediaTypes.Audio: return (this.PopoutView == null) ? "AudioIn" : "AudioOut";
                default: return string.Empty;
            }
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
            this.CurrentMedia = null; // Force player to stop
            if (this.PopoutView != null)
                this.PopoutView.Close();
        }
    }

    
}

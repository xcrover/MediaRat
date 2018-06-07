using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using Ops.NetCoe.LightFrame;
using System.Windows.Media.Imaging;

namespace MediOrg.Models {
    public class MediaRenameVModel : WorkspaceViewModel {
        ///<summary>Selected folder</summary>
        private string _currentFolder;
        ///<summary>Extensions</summary>
        private ObservableCollection<FileTypeDsc> _extensions= new ObservableCollection<FileTypeDsc>();
        ///<summary>Group divider treshold (minutes)</summary>
        private double _grpDivTreshold=20;
        ///<summary>File groups</summary>
        private ObservableCollection<FileGroupDsc> _fileGroups= new ObservableCollection<FileGroupDsc>();
        protected Analyser PCtx { get; set; }
        ///<summary>DateTime converter</summary>
        private DtConverter _grpDtConverter= new DtConverter();
        ///<summary>Files</summary>
        private ObservableCollection<FileDsc> _files= new ObservableCollection<FileDsc>();
        protected SlidingDelayAction DelayedRefresh;
        protected SlidingDelayAction DelayedImageRefresh;
        ///<summary>Counter format</summary>
        private string _counterFormat="000";
        ///<summary>Counter seed</summary>
        private int _cntSeed=1;
        ///<summary>Image to display</summary>
        private BitmapSource _currentImage;
        ///<summary>Current file description</summary>
        private FileDsc _currentFileDsc;
        ///<summary>Current group</summary>
        private FileGroupDsc _currentGroup;
        ///<summary>Is image selected</summary>
        private bool _isImageSelected;
        private HashSet<string> _imgExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        ///<summary>Name templates</summary>
        private ObservableCollection<CodeValuePair> _nameTemplates= new ObservableCollection<CodeValuePair>();
        ///<summary>Current name template</summary>
        private CodeValuePair _currentTemplate;
        ///<summary>Source marker</summary>
        private string _sourceMarker;
        ///<summary>Time shift</summary>
        private TimeSpan? _timeShift;

        #region Properties
        ///<summary>Current name template</summary>
        public CodeValuePair CurrentTemplate {
            get { return this._currentTemplate; }
            set {
                if (this._currentTemplate != value) {
                    this._currentTemplate = value;
                    this.FirePropertyChanged(nameof(CurrentTemplate));
                    PrepareNames();
                }
            }
        }

        ///<summary>Name templates</summary>
        public ObservableCollection<CodeValuePair> NameTemplates {
            get { return this._nameTemplates; }
            set {
                if (this._nameTemplates != value) {
                    this._nameTemplates = value;
                    this.FirePropertyChanged(nameof(NameTemplates));
                }
            }
        }


        ///<summary>Is image selected</summary>
        public bool IsImageSelected {
            get { return this._isImageSelected; }
            set {
                if (this._isImageSelected != value) {
                    this._isImageSelected = value;
                    this.FirePropertyChanged(nameof(IsImageSelected));
                }
            }
        }


        ///<summary>Current group</summary>
        public FileGroupDsc CurrentGroup {
            get { return this._currentGroup; }
            set {
                if (this._currentGroup != value) {
                    if (this._currentGroup != null)
                        this._currentGroup.IsAccented = false;
                    this._currentGroup = value;
                    if (this._currentGroup != null)
                        this._currentGroup.IsAccented = true;
                    this.FirePropertyChanged(nameof(CurrentGroup));
                }
            }
        }


        ///<summary>Current file description</summary>
        public FileDsc CurrentFileDsc {
            get { return this._currentFileDsc; }
            set {
                if (this._currentFileDsc != value) {
                    this._currentFileDsc = value;
                    this.FirePropertyChanged(nameof(CurrentFileDsc));
                    this.IsImageSelected = false;
                    this.DelayedImageRefresh.Slide();
                }
            }
        }


        ///<summary>Image to display</summary>
        public BitmapSource CurrentImage {
            get { return this._currentImage; }
            set {
                if (this._currentImage != value) {
                    this._currentImage = value;
                    this.FirePropertyChanged(nameof(CurrentImage));
                }
            }
        }


        ///<summary>Counter seed</summary>
        public int CntSeed {
            get { return this._cntSeed; }
            set {
                if (this._cntSeed != value) {
                    this._cntSeed = value;
                    this.FirePropertyChanged(nameof(CntSeed));
                    PrepareNames();
                }
            }
        }


        ///<summary>Counter format</summary>
        public string CounterFormat {
            get { return this._counterFormat; }
            set {
                if (this._counterFormat != value) {
                    this._counterFormat = value;
                    this.FirePropertyChanged(nameof(CounterFormat));
                    PrepareNames();
                }
            }
        }

        ///<summary>Source marker</summary>
        public string SourceMarker {
            get { return this._sourceMarker; }
            set {
                if (this._sourceMarker != value) {
                    this._sourceMarker = value;
                    this.FirePropertyChanged(nameof(SourceMarker));
                    PrepareNames();
                }
            }
        }

        ///<summary>Time shift</summary>
        public TimeSpan? TimeShift {
            get { return this._timeShift; }
            set {
                if (this._timeShift != value) {
                    this._timeShift = value;
                    this.FirePropertyChanged(nameof(TimeShift));
                    PrepareNames();
                }
            }
        }

        ///<summary>Files</summary>
        public ObservableCollection<FileDsc> Files {
            get { return this._files; }
            set {
                if (this._files != value) {
                    this._files = value;
                    this.FirePropertyChanged(nameof(Files));
                }
            }
        }


        ///<summary>DateTime converter</summary>
        public DtConverter GrpDtConverter {
            get { return this._grpDtConverter; }
            set {
                if (this._grpDtConverter != value) {
                    this._grpDtConverter = value;
                    this.FirePropertyChanged(nameof(GrpDtConverter));
                }
            }
        }


        ///<summary>File groups</summary>
        public ObservableCollection<FileGroupDsc>FileGroups {
            get { return this._fileGroups; }
            set {
                if (this._fileGroups != value) {
                    this._fileGroups = value;
                    this.FirePropertyChanged(nameof(FileGroups));
                }
            }
        }


        ///<summary>Group divider treshold (minutes)</summary>
        public double GrpDivTreshold {
            get { return this._grpDivTreshold; }
            set {
                if (this._grpDivTreshold != value) {
                    this._grpDivTreshold = value;
                    this.FirePropertyChanged(nameof(GrpDivTreshold));
                    this.DelayedRefresh.Slide();
                }
            }
        }


        ///<summary>Extensions</summary>
        public ObservableCollection<FileTypeDsc> Extensions {
            get { return this._extensions; }
            set {
                if (this._extensions != value) {
                    this._extensions = value;
                    this.FirePropertyChanged(nameof(Extensions));
                }
            }
        }


        ///<summary>Selected folder</summary>
        public string CurrentFolder {
            get { return this._currentFolder; }
            set {
                if (this._currentFolder != value) {
                    this._currentFolder = value;
                    this.FirePropertyChanged(nameof(CurrentFolder));
                }
            }
        }

        #endregion

        #region Construction

        public MediaRenameVModel() {
            this.Status = new StatusVModel();
            InitCommands();
            DelayedRefresh = new SlidingDelayAction(1000, RefreshA);
            DelayedImageRefresh = new SlidingDelayAction(700, RefreshImage);
            LoadImageExtensions();
            LoadNameTemplates();
        }

        void LoadImageExtensions() {
            this._imgExtensions.AddItems(".jpg", ".png", ".bmp", ".tiff", ".gif");
            var cfgExt= AppContext.Current.GetAppCfgItem("ImageExtensions");
            if (!string.IsNullOrEmpty(cfgExt)) {
                string[] parts = cfgExt.ToLower().Split(new char[] { ',', ';', ' ', '\t', '|' }, StringSplitOptions.RemoveEmptyEntries);
                string tmp;
                foreach(var x in parts) {
                    tmp = x.Trim();
                    if (!tmp.StartsWith(".")) {
                        tmp = "." + tmp;
                    }
                    this._imgExtensions.Add(tmp);
                }
            }
        }

         void LoadNameTemplates() {
            this.NameTemplates.Clear();
            this.NameTemplates.AddItems(
                new CodeValuePair("Date-Mrk-N-Group", "@FileDate @Marker@Counter @GroupName"),
                new CodeValuePair("Date-Group-FileName", "@FileDate @GroupName @FileName"),
                new CodeValuePair("Date-Mrk-Group-N", "@FileDate @Marker@GroupName @Counter")
            );

            char[] seps = new char[] { '|' };
            foreach(var t in (from ci in AppContext.Current.EnumerateAppCfgItems((k)=>k.StartsWith("fnmt.")) orderby ci.Code select ci)) {
                string[] parts = (t.Value ?? "").Split(seps, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length==2) { // Consider only valid definitions
                    this.NameTemplates.Add(new CodeValuePair(parts[0].Trim(), parts[1].Trim()));
                }
            }
            this.CurrentTemplate = this.NameTemplates[0];
        }

        #endregion

        #region Commands
        public ICommand CmdSelectSourceFolder { get; protected set; }
        public ICommand CmdApply { get; protected set; }
        public ICommand CmdStartGroup { get; protected set; }
        //public ICommand CmdEndGroup { get; protected set; }
        public ICommand CmdAddToPreviousGroup { get; protected set; }
        public ICommand CmdAddToNextGroup { get; protected set; }

        public void InitCommands() {
            this.CmdSelectSourceFolder = new DelegateCommand(DoSelectFolder);
            this.CmdApply = new DelegateCommand(DoApply, ()=>PCtx!=null);
            this.CmdStartGroup = new DelegateCommand(DoStartGroup, () => this.CurrentFileDsc != null);
            this.CmdAddToPreviousGroup = new DelegateCommand(DoAddToPreviousGroup, () => this.CurrentGroup != null);
            this.CmdAddToNextGroup = new DelegateCommand(DoAddToNextGroup, () => this.CurrentGroup != null);
        }
        #endregion

        void DoSelectFolder() {
            string src = this.CurrentFolder;
            if (AppContext.Current.GetServiceViaLocator<IUIHelper>().SelectFolder("Select source folder", ref src)) {
                this.CurrentFolder = src;
                Refresh();
            }
        }

        void DoApply() {
            this.Status.SetNeutral("Working...");
            if (this.PCtx != null) {
                var q = GetTargetFiles();
                string fnSrc="n/a", fnTrg="n/a";
                bool repeat;
                foreach(var f in q) {
                    repeat = true;
                    do {
                        try {
                            fnSrc = Path.Combine(PCtx.CurrentFolder, f.Name);
                            fnTrg = Path.Combine(PCtx.CurrentFolder, f.NewTitle+f.Extension);
                            File.Move(fnSrc, fnTrg);
                            f.Name = Path.GetFileName(fnTrg);
                            f.Title = Path.GetFileNameWithoutExtension(f.Name);
                            repeat = false;
                        }
                        catch (Exception x) {
                            string em = string.Format("Failed to rename file \"{0}\" to \"{1}\". {2}: {3}", fnSrc, fnTrg, x.GetType().Name, x.Message);
                            this.Status.SetError(em);
                            var rz= AppContext.Current.GetServiceViaLocator<IUIHelper>().GetStdRespond(string.Format("{0}\r\n\r\n\t\tRepeat?\r\n\r\nCancel will cancel the batch.", em));
                            switch(rz) {
                                case WellKnownResponds.No: repeat = false; break;
                                case WellKnownResponds.Yes: repeat = true; break;
                                case WellKnownResponds.Cancel: {
                                        this.Status.SetError("Rename job canceled");
                                    }
                                    return;
                            }
                        }
                    } while (repeat);
                }
            }
            this.Status.SetPositive("Done!");
        }

        void DoStartGroup() {
            var cfl = this.CurrentFileDsc;
            if (cfl!=null) {
                var cgrp = cfl.Group;
                int ix = this.FileGroups.IndexOf(cgrp);
                var ngrp = new FileGroupDsc() {
                    GrpName = GetNextGrpName(cgrp),
                     StartTime= new DateTimeRef(cfl.FileTime),
                     EndTime= new DateTimeRef(cgrp.EndTime.Value),
                     IsManual= true
                };
                this.FileGroups.Insert(ix + 1, ngrp);
                cgrp.EndTime = new DateTimeRef(cfl.FileTime.AddMilliseconds(-1));
                var qg = from f in this.Files where object.ReferenceEquals(f.Group,cgrp) select f;
                var cgMaxTm = cgrp.StartTime.Value; //need get more accurate endtime for the old group
                int cgn = 0, ngn = 0;
                foreach(var f in qg) {
                    if (f.FileTime<cgrp.EndTime.Value) {
                        cgn++;
                        if (cgMaxTm < f.FileTime)
                            cgMaxTm = f.FileTime;
                    }
                    else {
                        ngn++;
                        f.Group = ngrp;
                    }
                }
                cgrp.Count = cgn;
                cgrp.EndTime.Value = cgMaxTm;
                ngrp.Count = ngn;
                PrepareNames();
                this.CurrentGroup = ngrp;
            }
        }

        string GetNextGrpName(FileGroupDsc grp) {
            return grp.GrpName + ".a";
        }

        void DoAddToPreviousGroup() {
            var cgrp = this.CurrentGroup;
            if (cgrp == null) return;
            int ix = this.FileGroups.IndexOf(cgrp);
            if (ix < 1) return; // there is no previous group
            var pgrp = this.FileGroups[ix - 1];
            foreach(var f in this.EnumerateGroupFiles(cgrp)) {
                f.Group = pgrp;
            }
            pgrp.IsManual = true;
            pgrp.Count += cgrp.Count;
            pgrp.EndTime = cgrp.EndTime;
            this.CurrentGroup = pgrp;
            this.FileGroups.Remove(cgrp);
            Status.SetPositive(string.Format("Group \"{0}\" starting at {1:yyyy-MM-dd HH:mm:ss} has been added to \"{2}\"", 
                cgrp.GrpName, cgrp.StartTime.Value, pgrp.GrpName));
            PrepareNames();
        }

        void DoAddToNextGroup() {
            var cgrp = this.CurrentGroup;
            if (cgrp == null) return;
            int ix = this.FileGroups.IndexOf(cgrp);
            if ((ix < 0)||((ix+1)>=this.FileGroups.Count)) return; // there is no next group
            var pgrp = this.FileGroups[ix + 1];
            foreach (var f in this.EnumerateGroupFiles(cgrp)) {
                f.Group = pgrp;
            }
            pgrp.IsManual = true;
            pgrp.Count += cgrp.Count;
            pgrp.StartTime = cgrp.StartTime;
            this.CurrentGroup = pgrp;
            this.FileGroups.Remove(cgrp);
            Status.SetPositive(string.Format("Group \"{0}\" ending at {1:yyyy-MM-dd HH:mm:ss} has been added to \"{2}\"",
                cgrp.GrpName, cgrp.StartTime.Value, pgrp.GrpName));
            PrepareNames();
        }

        void OnFolderChange(string newTarget) {
            this.Status.Clear();
            if (string.IsNullOrEmpty(newTarget)) {
                this.FileGroups.Clear();
            }
            else {
                
            }
        }

        public void Refresh() {
            try {
                this.Extensions.Clear();
                this.FileGroups.Clear();
                this.Files.Clear();
                if (!string.IsNullOrEmpty(this.CurrentFolder)) {
                    if (Directory.Exists(this.CurrentFolder)) {
                        this.Status.SetNeutral("Processing...");
                        var files = new List<string>(Directory.EnumerateFiles(this.CurrentFolder));
                        this.PCtx = new Analyser(this.CurrentFolder, files);
                        (from t in PCtx.FTypes.Values orderby t.FileExt select t).Apply((a) => {
                            a.PropertyChanged += ParamChanged;
                            this.Extensions.Add(a);
                        });
                        (from f in this.PCtx.Files orderby f.FileTime, f.Extension select f).Apply(a=> {
                            this.Files.Add(a);
                        });
                        this.RefreshGroups();
                        this.Status.SetPositive(this.PCtx.ToString());
                    }
                    else {
                        this.Status.SetError(string.Format("Folder does not exists: \"{0}\"", this.CurrentFolder));
                    }
                }
            }
            catch(Exception x) {
                this.ReportError(x);
            }
        }

        IEnumerable<FileDsc> GetTargetFiles() {
            return (from f in PCtx.Files where f.Group != null orderby f.FileTime select f);
        }

        void RefreshGroups() {
            //System.Diagnostics.Debug.WriteLine(string.Format("RefreshGroups {0:HH:mm:ss.ffff}", DateTime.Now));
            if ((this.PCtx != null)&&(this.GrpDivTreshold>0)) {
                var times = this.PCtx.GetGroupTimes(this.GrpDivTreshold);
                this.FileGroups.Clear();
                this.PCtx.RecalcGroups(this.GrpDivTreshold).Apply((a) => {
                    a.PropertyChanged += GrpParamChanged;
                    this.FileGroups.Add(a);
                });
                //FileGroupDsc.GenerateSequence(times).Apply((a) => { this.FileGroups.Add(a); });
                PrepareNames();
            }
        }

        private void GrpParamChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            PrepareNames();
        }

        void PrepareNames() {
            if (PCtx != null) {
                var q = GetTargetFiles();
                Dictionary<string, string> rgNames = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                int ix = this.CntSeed;
                string txNm;
                DateTime dtn;
                TimeSpan dtDlt = this.TimeShift.HasValue ? this.TimeShift.Value : TimeSpan.Zero;
                StringBuilder template = new StringBuilder(this.CurrentTemplate.Value); // @FileDate @Counter @GroupName @FileName
                template = template.Replace("@FileDate", "{0:yyyy-MM-dd}")
                    .Replace("@Counter", string.Format("{{1:{0}}}", this.CounterFormat))
                    .Replace("@GroupName", "{2}")
                    .Replace("@FileName", "{3}")
                    .Replace("@Marker", this.SourceMarker??string.Empty);


                string nmFmt = template.ToString();
                foreach (var f in q) {
                    if (rgNames.TryGetValue(f.Title, out txNm)) {
                        f.NewTitle = txNm;
                    }
                    else {
                        dtn = f.FileTime;
                        if (dtDlt != TimeSpan.Zero)
                            dtn += dtDlt;
                        f.NewTitle = string.Format(nmFmt, dtn, ix++, f.Group.GrpName, f.Title);
                        rgNames[f.Title] = f.NewTitle;
                    }
                }
            }
        }

        private void ParamChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            this.DelayedRefresh.Slide();
        }

        public void AddIntervalUnits(int units) {
            double rz = this.GrpDivTreshold + units;
            this.GrpDivTreshold = Math.Max(1 / 60, rz);
        }

        void RefreshA() {
            this.RunOnUIThread(RefreshGroups);
        }

        IEnumerable<FileDsc> EnumerateGroupFiles(FileGroupDsc fgrp) {
            return (from f in this.Files where object.ReferenceEquals(f.Group, fgrp) select f);
        }

        void RefreshImage() {
            var fl = this.CurrentFileDsc;
            this.IsImageSelected = false;
            if ((fl!=null)&&(_imgExtensions.Contains(fl.Extension))) {
                Task<byte[]>.Factory.StartNew(() => LoadImage(Path.Combine(PCtx.CurrentFolder, fl.Name))).ContinueWith(t => {
                    if (t.IsFaulted) {
                        this.ReportError(t.Exception);
                    }
                    else {
                        this.RunOnUIThread(() => {
                            SetCurrentImageBytes(t.Result);
                        });
                    }
                });
            }
        }

        void SetCurrentImageBytes(byte[] imgBytes) {
            BitmapSource rz;
            try {
                rz = BitmapFrame.Create(new MemoryStream(imgBytes));
                rz.Freeze();
                var tmp = this.CurrentImage as IDisposable;
                this.CurrentImage = rz;
                this.IsImageSelected = this.CurrentImage != null;
            }
            catch (Exception x) {
                this.ReportError(x);
            }
        }

        byte[] LoadImage(string source) {
            BitmapImage rz = new BitmapImage();
            return File.ReadAllBytes(source);
        }

        public void RunDefaultActionOnCurrentMedia() {
            if (this.CurrentFileDsc!=null) {
                System.Diagnostics.Process.Start(Path.Combine(PCtx.CurrentFolder, this.CurrentFileDsc.Name));
            }
        }

        /// <summary>
        /// Find first file in the specified group
        /// </summary>
        /// <param name="grp"></param>
        /// <returns></returns>
        public FileDsc FindFirstFileForTheGroup(FileGroupDsc grp) {
            if (this.Files!=null) {
                return this.EnumerateGroupFiles(grp).FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Find first image file in the specified group
        /// </summary>
        /// <param name="grp"></param>
        /// <returns></returns>
        public FileDsc FindFirstImageForTheGroup(FileGroupDsc grp) {
            if (this.Files != null) {
                return this.EnumerateGroupFiles(grp).FirstOrDefault((fl)=> _imgExtensions.Contains(fl.Extension));
            }
            return null;
        }

        public override ICommand FindCommand(Func<ICommand, bool> filter) {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                this.DelayedRefresh.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}

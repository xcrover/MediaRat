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
        ///<summary>Group prefixes</summary>
        private ObservableCollection<CodeValuePair> _grpPrefixes= new ObservableCollection<CodeValuePair>();
        ///<summary>Current group prefix</summary>
        private CodeValuePair _currentGrpPrefix;

        ///<summary>Current group prefix</summary>
        public CodeValuePair CurrentGrpPrefix {
            get { return this._currentGrpPrefix; }
            set {
                if (this._currentGrpPrefix != value) {
                    this._currentGrpPrefix = value;
                    this.FirePropertyChanged(nameof(CurrentGrpPrefix));
                    PrepareNames();
                }
            }
        }


        ///<summary>Group prefixes</summary>
        public ObservableCollection<CodeValuePair> GrpPrefixes {
            get { return this._grpPrefixes; }
            set {
                if (this._grpPrefixes != value) {
                    this._grpPrefixes = value;
                    this.FirePropertyChanged(nameof(GrpPrefixes));
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

        public MediaRenameVModel() {
            this.Status = new StatusVModel();
            InitCommands();
            DelayedRefresh = new SlidingDelayAction(1000, RefreshA);
            DelayedImageRefresh = new SlidingDelayAction(700, RefreshImage);
            LoadImageExtensions();
            LoadGrpPrefixes();
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

        void LoadGrpPrefixes() {
            this.GrpPrefixes.Clear();
            string dflt = "{0:yyyy-MM-dd HH}";
            this.CurrentGrpPrefix = new CodeValuePair(dflt, "Date+h24");
            this.GrpPrefixes.AddItems(
                this.CurrentGrpPrefix,
                new CodeValuePair("{0:yyyy-MM-dd}", "Date"),
                new CodeValuePair("", "None")
                );
        }

        #region Commands
        public ICommand CmdSelectSourceFolder { get; protected set; }
        public ICommand CmdApply { get; protected set; }

        public void InitCommands() {
            this.CmdSelectSourceFolder = new DelegateCommand(SelectFolder);
            this.CmdApply = new DelegateCommand(Apply, ()=>PCtx!=null);
        }
        #endregion

        void SelectFolder() {
            string src = this.CurrentFolder;
            if (AppContext.Current.GetServiceViaLocator<IUIHelper>().SelectFolder("Select source folder", ref src)) {
                this.CurrentFolder = src;
                Refresh();
            }
        }

        void Apply() {
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
                            fnTrg = Path.Combine(PCtx.CurrentFolder, Path.ChangeExtension(f.NewTitle, f.Extension));
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
                string nmFmt = string.Format("{{0}} {{1:{0}}}", this.CounterFormat);
                string pfxFmt = this.CurrentGrpPrefix.Code;
                StringBuilder sb = new StringBuilder();
                foreach (var f in q) {
                    if (rgNames.TryGetValue(f.Title, out txNm)) {
                        f.NewTitle = txNm;
                    }
                    else {
                        sb.Clear();
                        if (!string.IsNullOrEmpty(pfxFmt)) {
                            sb.AppendFormat(pfxFmt, f.FileTime);
                            sb.Append(' ');
                        }
                        sb.AppendFormat(nmFmt, f.Group.GrpName, ix++);
                        f.NewTitle = sb.ToString();
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

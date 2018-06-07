using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Xml.XPath;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
    /// <summary>
    /// Media Project.
    /// This thing is all about ratings and categories.
    /// </summary>
    public class MediaProject : NotifyPropertyChangedBase, IXmlSource, IXmlConfigurable {
        ///<summary>Title</summary>
        private string _title= "MyProject";
        ///<summary>Description</summary>
        private string _description;
        ///<summary>Variables</summary>
        private ObservableCollection<CodeValuePair> _vars= new ObservableCollection<CodeValuePair>();
        ///<summary>Source Filters, i.e. filters to select source media files to populate the project</summary>
        private ObservableCollection<SourceFilter> _sourceFilters = new ObservableCollection<SourceFilter>();
        ///<summary>Media files</summary>
        private ObservableCollection<MediaFile> _mediaFiles=new ObservableCollection<MediaFile>();
        ///<summary>Rating definitions</summary>
        private ObservableCollection<RatingDefinition> _ratingDefinitions= new ObservableCollection<RatingDefinition>();
        ///<summary>Category definitions</summary>
        private ObservableCollection<CategoryDefinition> _categoryDefinitions= new ObservableCollection<CategoryDefinition>();
        ///<summary>Project Full file name</summary>
        private string _projectFileName;
        ///<summary>Video Helper</summary>
        private VideoHelper _videoHelper;
        ///<summary>Var dictionary</summary>
        private Dictionary<string, string> _varMap;

        ///<summary>Var dictionary</summary>
        public Dictionary<string, string> VarMap {
            get {
                if (this._varMap==null) {
                    this._varMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    foreach(var v in this.Vars) {
                        this._varMap[v.Code ?? string.Empty] = v.Value ?? string.Empty;
                    }
                }
                return this._varMap;
            }
            set { this._varMap = value; }
        }


        public const int OrderAutoStep = 10;

        #region Properties
        ///<summary>Project Full file name</summary>
        public string ProjectFileName {
            get { return this._projectFileName; }
            set {
                if (this._projectFileName != value) {
                    this._projectFileName = value;
                    this.FirePropertyChanged("ProjectFileName");
                }
            }
        }
       
        ///<summary>Media files</summary>
        public ObservableCollection<MediaFile> MediaFiles {
            get { return this._mediaFiles; }
            set {
                if (this._mediaFiles != value) {
                    this._mediaFiles = value;
                    this.FirePropertyChanged("MediaFiles");
                }
            }
        }
        

        ///<summary>Source Filters, i.e. filters to select source media files to populate the project</summary>
        public ObservableCollection<SourceFilter> SourceFilters {
            get { return this._sourceFilters; }
            set {
                if (this._sourceFilters != value) {
                    this._sourceFilters = value;
                    this.FirePropertyChanged("SourceFilters");
                }
            }
        }
        

        ///<summary>Variables</summary>
        public ObservableCollection<CodeValuePair> Vars {
            get { return this._vars; }
            set {
                if (this._vars != value) {
                    this._vars = value;
                    this.FirePropertyChanged("Vars");
                }
            }
        }


        ///<summary>Category definitions</summary>
        public ObservableCollection<CategoryDefinition> CategoryDefinitions {
            get { return this._categoryDefinitions; }
            set {
                if (this._categoryDefinitions != value) {
                    this._categoryDefinitions = value;
                    this.FirePropertyChanged("CategoryDefinitions");
                }
            }
        }


        ///<summary>Rating definitions</summary>
        public ObservableCollection<RatingDefinition> RatingDefinitions {
            get { return this._ratingDefinitions; }
            set {
                if (this._ratingDefinitions != value) {
                    this._ratingDefinitions = value;
                    this.FirePropertyChanged("RatingDefinitions");
                }
            }
        }

        ///<summary>Description</summary>
        public string Description {
            get { return this._description; }
            set {
                if (this._description != value) {
                    this._description = value;
                    this.FirePropertyChanged("Description");
                }
            }
        }
        

        ///<summary>Title</summary>
        public string Title {
            get { return this._title; }
            set {
                if (this._title != value) {
                    this._title = value;
                    this.FirePropertyChanged("Title");
                }
            }
        }

        ///<summary>Video Helper</summary>
        public VideoHelper VideoHelper {
            get { return this._videoHelper ?? (this._videoHelper = AppContext.Current.GetServiceViaLocator<VideoHelper>()); }
            set { this._videoHelper = value; }
        }
        
        #endregion

        #region XML Configuration
        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <param name="password">The password to be used to encrypt sensitive data. Can be <c>null</c>.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public XElement GetXml(string password = null) {
            XElement xdf, xcol, xrz = new XElement(XNames.xnMediaProject,
                new XAttribute(XNames.xaName, this.Title)).AddDescription(this.Description);
            xrz.Add(xdf= new XElement(XNames.xnDefinitions));
            // Save variables
            xdf.Add(xcol = new XElement(XNames.xnVars));
            foreach (var cv in this.Vars) {
                xcol.Add(new XElement(XNames.xnVar,
                    new XAttribute(XNames.xaId, cv.Code),
                    cv.Value ?? string.Empty));
            }
            // Save source filters
            xdf.Add(xcol = new XElement(XNames.xnSourceFilters));
            foreach (var cv in this.SourceFilters) {
                if (!string.IsNullOrEmpty(cv.Folder))
                    xcol.Add(cv.GetXml(password));
            }
            // Add category definitions
            xdf.Add(xcol = new XElement(XNames.xnCategories));
            foreach (var cv in this.CategoryDefinitions) {
                xcol.Add(cv.GetXml(password));
            }
            // Add rating definitions
            xdf.Add(xcol = new XElement(XNames.xnRatings));
            foreach (var cv in this.RatingDefinitions) {
                xcol.Add(cv.GetXml(password));
            }
            // Add source files
            xrz.Add(xcol = new XElement(XNames.xnMedia));
            foreach (var cv in this.MediaFiles) {
                xcol.Add(cv.GetXml(password));
            }
            return xrz;
        }

        /// <summary>
        /// Applies the configuration.
        /// </summary>
        /// <param name="configSource">The configuration source. Can be <c>null</c>.</param>
        /// <param name="password">The password to be used to decrypt sensitive data. Can be <c>null</c>.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ApplyConfiguration(XElement configSource, string password = null) {
            this.Clear();
            XElement xdf, xcol;
            this.Title= configSource.GetAttributeValue(XNames.xaName);
            this.Description= configSource.GetDescription();
            xdf = configSource.GetMandatoryElement(XNames.xnDefinitions, (em) => { throw new BizException(em); });
            // Read variables
            xcol = xdf.GetMandatoryElement(XNames.xnVars);
            foreach (var itm in from t in xcol.Elements(XNames.xnVar).Deserialize<CodeValuePair>(ToCodeValuePair) orderby t.Code select t) {
                this.Vars.Add(itm);
            }
            // Read source filters
            xcol = xdf.GetMandatoryElement(XNames.xnSourceFilters);
            foreach (var itm in from t in xcol.Elements(XNames.xnFilter).Deserialize<SourceFilter>(password) orderby t.Folder select t) {
                this.SourceFilters.Add(itm);
            }
            // Read category definitions
            xcol = xdf.GetMandatoryElement(XNames.xnCategories);
            foreach (var itm in from t in xcol.Elements(XNames.xnCategory).Deserialize<CategoryDefinition>(password) orderby t.Title select t) {
                this.CategoryDefinitions.Add(itm);
            }
            // Read rating definitions
            xcol = xdf.GetMandatoryElement(XNames.xnRatings);
            foreach (var itm in from t in xcol.Elements(XNames.xnRating).Deserialize<RatingDefinition>(password) orderby t.Title select t) {
                this.RatingDefinitions.Add(itm);
            }
            // Reaad media file entries
            xcol = configSource.GetMandatoryElement(XNames.xnMedia);
            foreach (var itm in from t in xcol.Elements(XNames.xnItem).Deserialize<MediaFile>(ToMediaFile) orderby t.Title select t) {
                this.MediaFiles.Add(itm);
            }

        }

        static CodeValuePair ToCodeValuePair(XElement src) {
            return new CodeValuePair() {
                Code = src.GetMandatoryAttribute(XNames.xaId, (em) => { throw new BizException(em); }),
                Value = src.Value
            };
        }

        /// <summary>
        /// Creates the media file.
        /// </summary>
        /// <param name="mediaType">Type of the media.</param>
        /// <returns></returns>
        public MediaFile CreateMediaFile(MediaTypes mediaType) {
            switch (mediaType) {
                case MediaTypes.Audio: return new AudioFile() { Project = this };
                case MediaTypes.Image: return new ImageFile() { Project = this };
                case MediaTypes.Video: return new VideoFile() { Project = this };
                default: return null;
            }
        }

        MediaFile ToMediaFile(XElement src) {
            var mediaType = src.GetMandatoryAttribute(XNames.xaType).ToEnum<MediaTypes>(MediaTypes.Undefined);
            MediaFile mf = this.CreateMediaFile(mediaType);
            mf.ApplyConfiguration(src);
            return mf;
        }
        #endregion

        #region Operations

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear() {
            this.Title = string.Empty;
            this.Description = string.Empty;
            this.Vars.Clear();
            this.SourceFilters.Clear();
            this.RatingDefinitions.Clear();
            this.CategoryDefinitions.Clear();
            this.MediaFiles.Clear();
        }

        /// <summary>
        /// Assign weights 
        /// </summary>
        public void AutoAssignOrder() {
            if (this.MediaFiles != null) {
                for (int i = 0; i < this.MediaFiles.Count; i++) {
                    this.MediaFiles[i].OrderWeight = (i+1) * MediaProject.OrderAutoStep;
                }
            }
        }


        /// <summary>
        /// Assign weights 
        /// </summary>
        public void AutoAssignOrder<Tkey>(Func<MediaFile, Tkey> keyGetter) {
            if (this.MediaFiles!=null) {
                int i = 1;
                foreach(var mf in this.MediaFiles.OrderBy(keyGetter)) {
                    mf.OrderWeight = i * MediaProject.OrderAutoStep;
                    i++;
                }
            }
        }


        #endregion
    }

    /// <summary>
    /// Media types
    /// </summary>
    public enum MediaTypes {
        /// <summary>
        /// The undefined
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// The image
        /// </summary>
        Image = 1,
        /// <summary>
        /// The video
        /// </summary>
        Video = 2,
        /// <summary>
        /// The audio
        /// </summary>
        Audio = 3
    }

    /// <summary>
    /// Media file description
    /// </summary>
    abstract public class MediaFile : NotifyPropertyChangedBase, IXmlSource, IXmlConfigurable, ISourceRef {
        ///<summary>Title (file name)</summary>
        private string _title;
        ///<summary>Full file name (can contain variables)</summary>
        private string _fullName;
        ///<summary>Description</summary>
        private string _description;
        ///<summary>Flag that indicates that file is present in the specified location (true if the file is present, false if not, null if not checked yet)</summary>
        private bool? _isLocated;
        ///<summary>Is marked</summary>
        private bool _isMarked;
        /////<summary>Media type</summary>
        //private MediaTypes _mediaType;
        ///<summary>Ratings</summary>
        private KeyValuePairXCol<RatingDefinition, int> _ratings = new KeyValuePairXCol<RatingDefinition, int>();
        ///<summary>Assigned categories</summary>
        private KeyValuePairXCol<CategoryDefinition, HashSet<string>> _categories = new KeyValuePairXCol<CategoryDefinition, HashSet<string>>();
        ///<summary>Media Project</summary>
        private MediaProject _project;
        ///<summary>Is rated. This flag indicates that this media has ratings and categories specified</summary>
        private bool _isRated;
        ///<summary>Additional Marker text</summary>
        private string _marker;
        ///<summary>Order weight</summary>
        private int _orderWeight;

        ///<summary>Order weight</summary>
        public int OrderWeight {
            get { return this._orderWeight; }
            set {
                if (this._orderWeight != value) {
                    this._orderWeight = value;
                    this.FirePropertyChanged("OrderWeight");
                }
            }
        }


        ///<summary>Additional Marker text</summary>
        public string Marker {
            get { return this._marker; }
            set {
                if (this._marker != value) {
                    this._marker = value;
                    this.FirePropertyChanged("Marker");
                }
            }
        }
        

        ///<summary>Is rated. This flag indicates that this media has ratings and categories specified</summary>
        public bool IsRated {
            get { return this._isRated; }
            set {
                if (this._isRated != value) {
                    this._isRated = value;
                    this.FirePropertyChanged("IsRated");
                }
            }
        }
        
        ///<summary>Media Project</summary>
        public MediaProject Project {
            get { return this._project; }
            set { this._project = value; }
        }

        ///<summary>Media type</summary>
        abstract public MediaTypes MediaType { get; }
        //    get { return this._mediaType; }
        //    set { this._mediaType = value; }
        //}
        
        ///<summary>Assigned categories</summary>
        public KeyValuePairXCol<CategoryDefinition, HashSet<string>> Categories {
            get { return this._categories; }
            //set { this._categories = value; }
        }
        

        ///<summary>Ratings</summary>
        public KeyValuePairXCol<RatingDefinition, int> Ratings {
            get { return this._ratings; }
            //set { this._ratings = value; }
        }
        

        ///<summary>Is marked</summary>
        public bool IsMarked {
            get { return this._isMarked; }
            set {
                if (this._isMarked != value) {
                    this._isMarked = value;
                    this.FirePropertyChanged("IsMarked");
                }
            }
        }
        

        ///<summary>Full file name (can contain variables)</summary>
        public string FullName {
            get { return this._fullName; }
            set {
                if (this._fullName != value) {
                    this._fullName = value;
                    this.FirePropertyChanged("FullName");
                }
            }
        }
        ///<summary>Flag that indicates that file is present in the specified location (true if the file is present, false if not, null if not checked yet)</summary>
        public bool? IsLocated {
            get { return this._isLocated; }
            set {
                if (this._isLocated != value) {
                    this._isLocated = value;
                    this.FirePropertyChanged("IsLocated");
                }
            }
        }
        

        ///<summary>Title (file name)</summary>
         public string Title {
            get { return this._title; }
            set {
                if (this._title != value) {
                    this._title = value;
                    this.FirePropertyChanged("Title");
                }
            }
        }


         ///<summary>Description</summary>
         public string Description {
             get { return this._description; }
             set {
                 if (this._description != value) {
                     this._description = value;
                     this.FirePropertyChanged("Description");
                 }
             }
         }

        /// <summary>
        /// Get full path to the source file. Implements <see cref="ISourceRef"/>.
        /// </summary>
        public string SourcePath {
            get { return GetFullPath(); }
        }

        #region XML Configuration
        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <param name="password">The password to be used to encrypt sensitive data. Can be <c>null</c>.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual XElement GetXml(string password = null) {
             XElement xcols, xnd, xrz = new XElement(XNames.xnItem,
                 new XAttribute(XNames.xaType, this.MediaType.ToString()),
                 new XAttribute(XNames.xaLocation, this.FullName));
            if (this.OrderWeight!=0) {
                xrz.Add(new XAttribute(XNames.xaOrderW, this.OrderWeight));
            }
             if (!string.IsNullOrWhiteSpace(this.Marker)) {
                 xrz.Add(new XAttribute(XNames.xaMarker, this.Marker));
             }
             foreach (var rt in this.Ratings) {
                 xrz.Add(new XAttribute(rt.Key.XaName, rt.Value));
             }
             xrz.AddDescription(this.Description);
             xrz.Add(xcols = new XElement(XNames.xnCategories));
             foreach (var cat in this.Categories) {
                 if ((cat.Value != null) && (cat.Value.Count > 0)) { 
                     xcols.Add(xnd = new XElement(XNames.xnCategory,
                         new XAttribute(XNames.xaId, cat.Key.Marker)));
                     foreach (var t in cat.Value) {
                         xnd.Add(new XElement(XNames.xnItem, t));
                     }
                 }
             }
             return xrz;
         }

        /// <summary>
        /// Applies the configuration.
        /// </summary>
        /// <param name="configSource">The configuration source. Can be <c>null</c>.</param>
        /// <param name="password">The password to be used to decrypt sensitive data. Can be <c>null</c>.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void ApplyConfiguration(XElement configSource, string password = null) {
            this.FullName = configSource.GetMandatoryAttribute(XNames.xaLocation);
            this.Title = System.IO.Path.GetFileNameWithoutExtension(this.FullName);
            this.OrderWeight = configSource.GetAttributeInt(XNames.xaOrderW, 0).Value;
            this.Marker = configSource.GetAttributeValue(XNames.xaMarker, null);
            this.Description = configSource.GetDescription();
            //XAttribute xa;
            string stm;
            int ntm;
            // Read ratings
            foreach (var rd in this.EnumerateRatingDefinitions()) {
                stm = configSource.GetAttributeValue(rd.XaName);
                if (int.TryParse(stm, out ntm)) {
                    this.Ratings.Add(new KeyValuePairX<RatingDefinition, int>() {
                        Key = rd,
                        Value = ntm
                    });
                }
            }
            XElement xcats = configSource.Element(XNames.xnCategories);
            if (xcats != null) {
                foreach(var ctd in from c in xcats.Elements(XNames.xnCategory).Deserialize<KeyValuePairX<CategoryDefinition, HashSet<string>>>(ToAssignedCats) select c) {
                    this.Categories.Add(ctd);
                }
            }
            RefreshIsRated();
        }

        #endregion

        #region Operations

        /// <summary>
        /// Get explicit full path to the file
        /// </summary>
        /// <returns></returns>
        public string GetFullPath() {
            return System.IO.Path.GetFullPath(this.FullName);
        }

        IEnumerable<RatingDefinition> EnumerateRatingDefinitions() {
            foreach (var rd in this.Project.RatingDefinitions)
                yield return rd;
        }

        IEnumerable<CategoryDefinition> EnumerateCatDefinitions() {
            foreach (var cd in this.Project.CategoryDefinitions)
                yield return cd;
        }

        KeyValuePairX<CategoryDefinition, HashSet<string>> ToAssignedCats(XElement src) {
            string cn = src.GetMandatoryAttribute(XNames.xaId, (em) => { throw new BizException(em); });
            CategoryDefinition cd = EnumerateCatDefinitions().FirstOrDefault((d) => d.Marker == cn);
            if (cd == null)
                throw new BizException(string.Format("Failed to identify category by id= '{0}'", cn));
            KeyValuePairX<CategoryDefinition, HashSet<string>> rz = new KeyValuePairX<CategoryDefinition, HashSet<string>>() {
                Key = cd,
                Value = new HashSet<string>()
            };
            foreach (var xi in src.Elements(XNames.xnItem)) {
                if (!string.IsNullOrEmpty(xi.Value)) {
                    rz.Value.Add(xi.Value);
                }
            }
            return rz;
        }

        /// <summary>
        /// Refreshes the is rated flag.
        /// </summary>
        /// <returns></returns>
        public bool RefreshIsRated() {
            return this.IsRated = (this.Ratings.Count > 0) || this.HasAssignedCategory();
        }

        /// <summary>
        /// Determines whether this media has at least one assigned category.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this media has at least one assigned category; otherwise, <c>false</c>.
        /// </returns>
        public bool HasAssignedCategory() {
            foreach (var ca in this.Categories) {
                if (ca.Value.Count > 0) return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the content metadata. This is heavy operation as it can require extracting 
        /// metadata from the actual media file.
        /// </summary>
        /// <returns></returns>
        abstract public KeyValuePairXCol<string, string> GetMetadata();

        /// <summary>
        /// Gets the media list entry.
        /// Media List contains human-friendly information.
        /// The idea is to use it to prepare data sheets for film editing.
        /// </summary>
        /// <returns></returns>
        virtual public XElement GetMediaListEntry() {
            XElement xr= new XElement(XNames.xnItem,
                new XAttribute(XNames.xaType, this.MediaType.ToString()),
                new XAttribute(XNames.xaName, this.Title));
            if (this.OrderWeight!=0) {
                xr.Add(new XAttribute(XNames.xaOrderW, this.OrderWeight));
            }
            if (!string.IsNullOrWhiteSpace(this.Marker)) {
                xr.Add(new XAttribute(XNames.xaMarker, this.Marker));
            }
            StringBuilder sb = new StringBuilder();
            foreach (var rt in this.Ratings) {
                sb.Append(rt.Key.Marker).Append(rt.Value);
            }
            if (sb.Length > 0)
                xr.Add(new XAttribute("ratings", sb.ToString()));
            foreach (var cat in this.Categories) {
                if ((cat.Value != null) && (cat.Value.Count > 0)) {
                    var cq = from t in cat.Value orderby t select t;
                    xr.Add(new XAttribute("ctg_" + cat.Key.Marker, string.Join("|", cq)));
                }
            }
            xr.AddDescription(this.Description);
            return xr;
        }

        /// <summary>
        /// Enumerates the property elements.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PropElement> EnumeratePropElements() {
            PropElement tm;
            tm = new PropElement() {
                 Title= "Marker",
                 Tag= this.Marker,
                 ValueText= this.Marker,
                 IsMarked= !string.IsNullOrEmpty(this.Marker),
                 Applicator= (p,mf)=> mf.Marker= p.ValueText
            };
            yield return tm;
            tm = new PropElement() {
                Title = "Memo",
                Tag = this.Description,
                ValueText = this.Description,
                IsMarked = !string.IsNullOrEmpty(this.Description),
                Applicator = (p, mf) => mf.Description = p.ValueText
            };
            yield return tm;
            foreach (var rt in this.Ratings) {
                if (rt.Value > 0) {
                    tm = new PropElement() {
                        Title = rt.Key.Title,
                        Tag = rt.Value,
                        TypeRef= rt.Key,
                        ValueText = rt.Value.ToString(),
                        IsMarked = true,
                        Applicator = (p, mf) => mf.EnsureRating((RatingDefinition)p.TypeRef, (int)p.Tag)
                    };
                    yield return tm;
                }
            }
            foreach (var cat in this.Categories) {
                if ((cat.Value != null) && (cat.Value.Count > 0)) {
                    foreach(var cv in from c in cat.Value orderby cat select c) {
                        tm = new PropElement() {
                            Title = cat.Key.Title,
                            Tag = cv,
                            TypeRef = cat.Key,
                            ValueText = cv,
                            IsMarked = true,
                            Applicator = (p, mf) => mf.EnsureCategory((CategoryDefinition)p.TypeRef, p.ValueText)
                        };
                        yield return tm;

                    }
                }
            }
        }

        /// <summary>
        /// Applies the property elements.
        /// </summary>
        /// <param name="elements">The elements.</param>
        public void ApplyPropElements(IEnumerable<PropElement> elements) {
            foreach (var pe in elements) {
                pe.Applicator(pe, this);
                this.RefreshIsRated();
            }
        }

        /// <summary>
        /// Ensures the rating.
        /// </summary>
        /// <param name="rdef">The rdef.</param>
        /// <param name="value">The value.</param>
        public void EnsureRating(RatingDefinition rdef, int value) {
            var rv = this.Ratings.FirstOrDefault((r) => r.Key == rdef);
            if (rv == null) {
                this.Ratings.Add(new KeyValuePairX<RatingDefinition, int>() { Key= rdef, Value= value});
            }
            else {
                rv.Value = value;
            }
        }

        /// <summary>
        /// Ensures the category.
        /// </summary>
        /// <param name="cdef">The cdef.</param>
        /// <param name="value">The value.</param>
        public void EnsureCategory(CategoryDefinition cdef, string value) {
            var ctSet = this.Categories.FirstOrDefault((c) => c.Key == cdef);
            if (ctSet == null) {
                this.Categories.Add(ctSet= new KeyValuePairX<CategoryDefinition, HashSet<string>>() { Key = cdef, Value = new HashSet<string>() });
            }
            if (ctSet.Value == null)
                ctSet.Value = new HashSet<string>();
            ctSet.Value.Add(value);
        }
        #endregion
    }

    /// <summary>
    /// Media file with video
    /// </summary>
    public class VideoFile : MediaFile {
        ///<summary>Video Format</summary>
        private string _mediaFormat;
        ///<summary>Duration</summary>
        private TimeSpan? _duration;
        ///<summary>Bitrate</summary>
        private int? _bitrate;

        ///<summary>Bitrate</summary>
        public int? Bitrate {
            get { return this._bitrate; }
            set {
                if (this._bitrate != value) {
                    this._bitrate = value;
                    this.FirePropertyChanged("Bitrate");
                }
            }
        }
        

        ///<summary>Duration</summary>
        public TimeSpan? Duration {
            get { return this._duration; }
            set {
                if (this._duration != value) {
                    this._duration = value;
                    this.FirePropertyChanged("Duration");
                }
            }
        }
        

        ///<summary>Media Format</summary>
        public string MediaFormat {
            get { return this._mediaFormat; }
            set {
                if (this._mediaFormat != value) {
                    this._mediaFormat = value;
                    this.FirePropertyChanged("MediaFormat");
                }
            }
        }
        
        ///<summary>Media type</summary>
        override public MediaTypes MediaType {
            get { return MediaTypes.Video; }
        }


        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <param name="password">The password to be used to encrypt sensitive data. Can be <c>null</c>.</param>
        /// <returns></returns>
        public override XElement GetXml(string password = null) {
            var xe= base.GetXml(password);
            AddVideoAttributes(xe);
            return xe;
        }

        private void AddVideoAttributes(XElement xe) {
            if (!string.IsNullOrEmpty(this.MediaFormat))
                xe.Add(new XAttribute(XNames.xaMediaFormat, this.MediaFormat));
            if (this.Duration.HasValue)
                xe.Add(new XAttribute(XNames.xaDuration, this.Duration.Value));
            if (this.Bitrate.HasValue)
                xe.Add(new XAttribute(XNames.xaBitrate, this.Bitrate.Value));
        }

        /// <summary>
        /// Applies the configuration.
        /// </summary>
        /// <param name="configSource">The configuration source. Can be <c>null</c>.</param>
        /// <param name="password">The password to be used to decrypt sensitive data. Can be <c>null</c>.</param>
        public override void ApplyConfiguration(XElement configSource, string password = null) {
            base.ApplyConfiguration(configSource, password);
            this.MediaFormat= configSource.GetAttributeValue(XNames.xaMediaFormat, null);
            this.Duration = configSource.GetAttributeTimeSpan(XNames.xaDuration);
            this.Bitrate = configSource.GetAttributeInt(XNames.xaBitrate);
        }

        /// <summary>
        /// Gets the media list entry.
        /// Media List contains human-friendly information.
        /// The idea is to use it to prepare data sheets for film editing.
        /// </summary>
        /// <returns></returns>
        public override XElement GetMediaListEntry() {
            var xr= base.GetMediaListEntry();
            AddVideoAttributes(xr);
            if (this.Duration.HasValue) {
                xr.Add(new XAttribute(XNames.xaDurationS, this.Duration.Value.ToHumanString()));
            }
            return xr;
        }

        /// <summary>
        /// Gets the content metadata. This is heavy operation as it can require extracting 
        /// metadata from the actual media file.
        /// </summary>
        /// <returns></returns>
        public override KeyValuePairXCol<string, string> GetMetadata() {
            KeyValuePairXCol<string, string> rz = null;
            VideoHelper vhl = this.Project.VideoHelper;
            try {
                rz = vhl.GetMetadata(this);
                if (rz != null) { // Autoupdate properties
                    var kvp = rz.FirstOrDefault((r) => r.Key == VideoHelper.cnDuration);
                    if (kvp != null) {
                        TimeSpan tmp;
                        if (TimeSpan.TryParse(kvp.Value, out tmp)) {
                            this.Duration = tmp;
                        }
                    }
                    if (null!=(kvp= rz.FirstOrDefault((r)=>r.Key==VideoHelper.cnMediaFormat))) {
                        this.MediaFormat= kvp.Value;
                    }
                    if (null != (kvp = rz.FirstOrDefault((r) => r.Key == VideoHelper.cnBitrate))) {
                        int tmp;
                        if (int.TryParse(kvp.Value, out tmp)) {
                            this.Bitrate = tmp;
                        }
                    }
                }
            }
            catch (Exception x) {
                rz = new KeyValuePairXCol<string, string>();
                rz.Add("Error", x.ToShortMsg());
            }
            return rz;
        }

    }

    /// <summary>
    /// Media file with image
    /// </summary>
    public class ImageFile : MediaFile {
        ///<summary>Media type</summary>
        override public MediaTypes MediaType {
            get { return MediaTypes.Image; }
        }

        /// <summary>
        /// Gets the image data.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Ops.NetCoe.LightFrame.BizException"></exception>
        public ImageData GetImageDataX() {
            ImageData rz = new ImageData() { FileName = this.FullName };
            BitmapSource img = null;
            try {
                byte[] buffer = File.ReadAllBytes(this.FullName);
                MemoryStream memoryStream = new MemoryStream(buffer);
                img = BitmapFrame.Create(memoryStream);
                rz.MediaProps = GetMetadata(img.Metadata as BitmapMetadata);
                img.Freeze();
                rz.Content = img;
            }
            catch (Exception x) {
                throw new BizException(string.Format("Failed to load image from \"{0}\". {1}: {2}", this.FullName, x.GetType().Name, x.Message));
            }
            return rz;
        }

        /// <summary>
        /// Gets the image data.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Ops.NetCoe.LightFrame.BizException"></exception>
        public ImageData GetImageData() {
            string fpath = this.GetFullPath();
            ImageData rz = new ImageData() { FileName = fpath };
            BitmapSource img = null;
            try {
                byte[] buffer = File.ReadAllBytes(fpath);
                MemoryStream memoryStream = new MemoryStream(buffer);
                img = BitmapFrame.Create(memoryStream);
                //memoryStream.Seek(0, SeekOrigin.Begin);
                rz.MediaProps = MediaUtil.GetMetadata(fpath);
                img.Freeze();
                rz.Content = img;
            }
            catch (Exception x) {
                throw new BizException(string.Format("Failed to load image from \"{0}\". {1}: {2}", this.FullName, x.GetType().Name, x.Message));
            }
            return rz;
        }


        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <param name="imgData">The img data.</param>
        /// <returns></returns>
        /// <exception cref="Ops.NetCoe.LightFrame.BizException"></exception>
        public KeyValuePairXCol<string, string> GetMetadata(ImageMetadata imgData) {
            KeyValuePairXCol<string, string> rz = new KeyValuePairXCol<string, string>();
            object tmp;
            try {
                BitmapMetadata mData = imgData as BitmapMetadata;
                if (mData != null) {
                    rz.Add("Camera", string.Format("{1} [{0}]", mData.CameraManufacturer, mData.CameraModel));
                    if (null != (tmp = mData.GetQuery("System.Photo.LensModel")??GetExifStr(mData, Constants.ExifQuery.SonyLens)))
                        rz.Add("Lens", tmp.ToString());

                    ImageDim dm;
                    if (null!=(dm= GetExifDim(mData))) {
                        rz.Add("Dimension [px]", dm.ToString());
                    }

                    rz.Add("Application", mData.ApplicationName);
                    rz.Add("Date taken", mData.DateTaken);
                    rz.Add("Format", mData.Format);
                    rz.Add("Title", mData.Title);
                    rz.Add("Subject", mData.Subject);
                    rz.Add("Location", mData.Location);
                    rz.Add("Rating", mData.Rating.ToString());
                    if (mData.Keywords != null) {
                        rz.Add("Keywords", string.Join(", ", mData.Keywords));
                    }
                    rz.Add("Comment", mData.Comment);
                    //foreach (var ms in mData) {
                    //    rz.Add("", ms);
                    //}
                    //object tmp= mData.GetQuery("/app1/ifd/exif/{ushort=37378}") ?? mData.GetQuery("/xmp/exif:ApertureValue");
                    //object tmp = mData.GetQuery("System.Photo.Aperture");
                    if (null != (tmp = mData.GetQuery("System.Photo.FNumber")))
                        rz.Add("FNumber", tmp.ToString());
                    if (null!= (tmp = mData.GetQuery("System.Photo.Aperture")))
                        rz.Add("Apperture", tmp.ToString());
                    //if (null!=(tmp= mData.GetQuery("/app1/ifd/exif/{ushort=33434}") ?? mData.GetQuery("/xmp/exif:ExposureTime")))
                    if (null != (tmp = mData.GetQuery("System.Photo.ExposureTime")))
                        rz.Add("Exposure", tmp.ToString());
                    if (null != (tmp = mData.GetQuery("System.Photo.SubjectDistance")))
                        rz.Add("Distance", tmp.ToString());
                    if (null != (tmp = mData.GetQuery("System.Photo.WhiteBalance")))
                        rz.Add("White balance", tmp.ToString());
                    if (null != (tmp = mData.GetQuery("System.Photo.Orientation")))
                        rz.Add("Orientation", tmp.ToString());
                    if (null != (tmp = mData.GetQuery("System.Photo.ISOSpeed")))
                        rz.Add("ISO Speed", tmp.ToString());
                    if (null != (tmp = mData.GetQuery("System.Photo.FocalLength")))
                        rz.Add("Focal Length", tmp.ToString());
                    if (null != (tmp = mData.GetQuery("System.Photo.FocalLengthInFilm")))
                        rz.Add("35mm Focal Length", tmp.ToString());
                    DumpMeta(mData);
                }
            }
            catch (Exception x) {
                throw new BizException(string.Format("Failed to extract properties. {0}: {1}", x.GetType().Name, x.Message));
            }
            return rz;
        }

        string GetExifStr(BitmapMetadata mData, string query) {
            try {
                object tp = mData.GetQuery(query);
                if (tp == null) return null;
                BitmapMetadataBlob bmBlob = tp as BitmapMetadataBlob;
                if (bmBlob==null) {
                    return tp.ToString();
                }
                else
                    return ASCIIEncoding.Default.GetString(bmBlob.GetBlobValue());
            }
            catch {
                return null;
            }
        }


        int? GetExifInt(BitmapMetadata mData, string query) {
            var s = GetExifStr(mData, query);
            int rz;
            if (string.IsNullOrEmpty(s)) return null;
            if (int.TryParse(s, out rz)) return rz;
            return null;
        }

        uint? GetExifUInt(BitmapMetadata mData, string query) {
            var s = GetExifStr(mData, query);
            uint rz;
            if (string.IsNullOrEmpty(s)) return null;
            if (uint.TryParse(s, out rz)) return rz;
            return null;
        }


        ImageDim GetExifDim(BitmapMetadata mData) {
            uint? w = GetExifUInt(mData, Constants.ExifQuery.WidthPix);
            if (w.HasValue) {
                uint? h= GetExifUInt(mData, Constants.ExifQuery.HeightPix);
                if (h.HasValue) {
                    return new ImageDim() { Width = w.Value, Height = h.Value };
                }
            }
            return null;
        }

        /*
        https://msdn.microsoft.com/en-us/library/system.drawing.imaging.propertyitem.id(v=vs.110).aspx
        EXIF Sony tags: http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/Sony.html
        EXIF Sony tags: http://exiv2.org/tags-sony.html
        Read EXIF (C#) without reading the whole image: http://www.codeproject.com/Articles/36342/ExifLib-A-Fast-Exif-Data-Extractor-for-NET-2-0
        [/{ushort=33434}]->System.UInt64 [1717986918401]
        [/{ushort=33437}]->System.UInt64 [42949673023]
        [/{ushort=34850}]->System.UInt16 [4]
        [/{ushort=34855}]->System.UInt16 [100]
        [/{ushort=34864}]->System.UInt16 [2]
        [/{ushort=34866}]->System.UInt32 [100]
        [/{ushort=36864}]->System.Windows.Media.Imaging.BitmapMetadataBlob [0230]
        [/{ushort=36867}]->System.String [2016:04:15 23:35:41]
        [/{ushort=36868}]->System.String [2016:04:15 23:35:41]
        [/{ushort=37121}]->System.Windows.Media.Imaging.BitmapMetadataBlob [[/{ushort=37122}]->System.UInt64 [4294967297]
        [/{ushort=37379}]->System.Int64 [10995116281424]
        [/{ushort=37380}]->System.Int64 [42949672960]
        [/{ushort=37381}]->System.UInt64 [1099511629136]
        [/{ushort=37383}]->System.UInt16 [5]
        [/{ushort=37384}]->System.UInt16 [0]
        [/{ushort=37385}]->System.UInt16 [16]
        [/{ushort=37386}]->System.UInt64 [42949675060]
        [/{ushort=37500}]->System.Windows.Media.Imaging.BitmapMetadataBlob [SONY DSC J[/{ushort=37510}]->System.Windows.Media.Imaging.BitmapMetadataBlob [[/{ushort=40960}]->System.Windows.Media.Imaging.BitmapMetadataBlob [0100]
        [/{ushort=40961}]->System.UInt16 [1]
        [/{ushort=40962}]->System.UInt32 [6000]
        [/{ushort=40963}]->System.UInt32 [3376]
        [/{ushort=40965}]->System.Windows.Media.Imaging.BitmapMetadata [System.Windows.Media.Imaging.BitmapMetadata]
        [/{ushort=41728}]->System.Windows.Media.Imaging.BitmapMetadataBlob []
        [/{ushort=41729}]->System.Windows.Media.Imaging.BitmapMetadataBlob []
        [/{ushort=41985}]->System.UInt16 [0]
        [/{ushort=41986}]->System.UInt16 [0]
        [/{ushort=41987}]->System.UInt16 [0]
        [/{ushort=41988}]->System.UInt64 [68719476752]
        [/{ushort=41989}]->System.UInt16 [315]
        [/{ushort=41990}]->System.UInt16 [0]
        [/{ushort=41992}]->System.UInt16 [0]
        [/{ushort=41993}]->System.UInt16 [0]
        [/{ushort=41994}]->System.UInt16 [0]
        [/{ushort=42034}]->System.UInt64[] [System.UInt64[]]
        [/{ushort=42036}]->System.String [E 55-210mm F4.5-6.3 OSS]
        */
        void DumpMeta(BitmapMetadata mData) {
            string mqs, mq = "/app1/ifd/exif";
            BitmapMetadata bm = mData.GetQuery(mq) as BitmapMetadata;
            BitmapMetadataBlob bmBlob;
            object tp;
            if (bm!=null) {
                foreach(var r in bm) {
                    tp = bm.GetQuery(r);
                    if (tp == null) {
                        System.Diagnostics.Debug.WriteLine(string.Format("[{0}]->NULL", r));
                    }
                    else {
                        bmBlob = tp as BitmapMetadataBlob;
                        if (bmBlob == null) {
                            System.Diagnostics.Debug.WriteLine(string.Format("[{0}]->{1} [{2}]", r, tp.GetType().FullName, tp));
                        }
                        else {
                            System.Diagnostics.Debug.WriteLine(string.Format("[{0}]->{1} [{2}]", r, tp.GetType().FullName, ASCIIEncoding.Default.GetString(bmBlob.GetBlobValue())));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the content metadata. This is heavy operation as it can require extracting 
        /// metadata from the actual media file.
        /// </summary>
        /// <returns></returns>
        public override KeyValuePairXCol<string, string> GetMetadata() {
            KeyValuePairXCol<string, string> rz = null;
            BitmapSource img = null;
            try {
                byte[] buffer = File.ReadAllBytes(this.FullName);
                MemoryStream memoryStream = new MemoryStream(buffer);
                img = BitmapFrame.Create(memoryStream);
                rz = GetMetadata(img.Metadata as BitmapMetadata);
            }
            catch (Exception x) {
                rz = new KeyValuePairXCol<string, string>();
                rz.Add("Error", string.Format("Failed to load image from \"{0}\". {1}: {2}", this.FullName, x.GetType().Name, x.Message));
            }
            return rz;
        }

    }

    /// <summary>
    /// Media file with audio
    /// </summary>
    public class AudioFile : MediaFile {
        ///<summary>Media type</summary>
        override public MediaTypes MediaType {
            get { return MediaTypes.Audio; }
        }

        /// <summary>
        /// Gets the content metadata. This is heavy operation as it can require extracting 
        /// metadata from the actual media file.
        /// </summary>
        /// <returns></returns>
        public override KeyValuePairXCol<string, string> GetMetadata() {
            KeyValuePairXCol<string, string> rz = new KeyValuePairXCol<string,string>();
            return rz;
        }

    }


    /// <summary>
    /// Rating definition. Rating is a digit [0..9] with marker.
    /// </summary>
    public class RatingDefinition : NotifyPropertyChangedBase, IXmlSource, IXmlConfigurable {
        ///<summary>Title</summary>
        private string _title;
        ///<summary>Description</summary>
        private string _description;
        ///<summary>Marker is a short string key that is used to prefix the rating value</summary>
        private string _marker;
        ///<summary>Rating XML attribute name</summary>
        private XName _xaName;

        ///<summary>Rating XML attribute name</summary>
        public XName XaName {
            get { return this._xaName; }
            protected set { this._xaName = value; }
        }
        

        ///<summary>Marker is a short string key that is used to prefix the rating value</summary>
        public string Marker {
            get { return this._marker; }
            set {
                if (this._marker != value) {
                    this._marker = value;
                    this.XaName = XNames.GetXaRatingName(this._marker);
                    this.FirePropertyChanged("Marker");
                }
            }
        }
        

        ///<summary>Description</summary>
        public string Description {
            get { return this._description; }
            set {
                if (this._description != value) {
                    this._description = value;
                    this.FirePropertyChanged("Description");
                }
            }
        }
        

        ///<summary>Title</summary>
        public string Title {
            get { return this._title; }
            set {
                if (this._title != value) {
                    this._title = value;
                    this.FirePropertyChanged("Title");
                }
            }
        }
        
        #region XML Configuration
        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <param name="password">The password to be used to encrypt sensitive data. Can be <c>null</c>.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public XElement GetXml(string password = null) {
            XElement xrz = new XElement(XNames.xnRating,
                new XAttribute(XNames.xaId, this.Marker),
                new XAttribute(XNames.xaName, this.Title));
            xrz.AddDescription(this.Description);
            return xrz;
        }

        /// <summary>
        /// Applies the configuration.
        /// </summary>
        /// <param name="configSource">The configuration source. Can be <c>null</c>.</param>
        /// <param name="password">The password to be used to decrypt sensitive data. Can be <c>null</c>.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ApplyConfiguration(XElement configSource, string password = null) {
            this.Marker = configSource.GetMandatoryAttribute(XNames.xaId);
            this.Title = configSource.GetMandatoryAttribute(XNames.xaName);
            this.Description = configSource.GetDescription();
        }
        #endregion
    }

    /// <summary>
    /// Category definition
    /// </summary>
    public class CategoryDefinition : NotifyPropertyChangedBase, IXmlSource, IXmlConfigurable {
        ///<summary>Title</summary>
        private string _title;
        ///<summary>Description</summary>
        private string _description;
        ///<summary>Marker is a short string key that is used to designate the category value</summary>
        private string _marker;
        ///<summary>Available cvalues</summary>
        private ObservableCollection<string> _values;

        ///<summary>Available cvalues</summary>
        public ObservableCollection<string> Values {
            get { return this._values; }
            set {
                if (this._values != value) {
                    this._values = value;
                    this.FirePropertyChanged("Values");
                }
            }
        }

        /// <summary>
        /// Gets the values safe.
        /// </summary>
        /// <value>
        /// The values safe.
        /// </value>
        public ObservableCollection<string> ValuesSafe {
            get { return this._values??(this.Values=new ObservableCollection<string>()); }
        }

        ///<summary>Marker is a short string key that is used to designate the category value</summary>
        public string Marker {
            get { return this._marker; }
            set {
                if (this._marker != value) {
                    this._marker = value;
                    this.FirePropertyChanged("Marker");
                }
            }
        }


        ///<summary>Description</summary>
        public string Description {
            get { return this._description; }
            set {
                if (this._description != value) {
                    this._description = value;
                    this.FirePropertyChanged("Description");
                }
            }
        }


        ///<summary>Title</summary>
        public string Title {
            get { return this._title; }
            set {
                if (this._title != value) {
                    this._title = value;
                    this.FirePropertyChanged("Title");
                }
            }
        }

        #region XML Configuration
        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <param name="password">The password to be used to encrypt sensitive data. Can be <c>null</c>.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public XElement GetXml(string password = null) {
            XElement xcols, xrz = new XElement(XNames.xnCategory,
                new XAttribute(XNames.xaId, this.Marker),
                new XAttribute(XNames.xaName, this.Title));
            xrz.AddDescription(this.Description);
            xrz.Add(xcols = new XElement(XNames.xnItems));
            if (this.Values != null) {
                foreach (var t in this.Values) {
                    xcols.Add(new XElement(XNames.xnItem,
                        new XAttribute(XNames.xaId, t)));
                }
            }
            return xrz;
        }

        /// <summary>
        /// Applies the configuration.
        /// </summary>
        /// <param name="configSource">The configuration source. Can be <c>null</c>.</param>
        /// <param name="password">The password to be used to decrypt sensitive data. Can be <c>null</c>.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ApplyConfiguration(XElement configSource, string password = null) {
            this.Marker = configSource.GetMandatoryAttribute(XNames.xaId);
            this.Title = configSource.GetMandatoryAttribute(XNames.xaName);
            this.Description = configSource.GetDescription();
            XElement xcols = configSource.Element(XNames.xnItems);
            if (xcols != null) {
                HashSet<string> cats = new HashSet<string>(); // Uniqueness check
                foreach (var cat in from t in xcols.Elements(XNames.xnItem).Deserialize<string>(ToIdString) orderby t select t) {
                    if (cats.Add((cat??string.Empty).ToLower()))
                        this.ValuesSafe.Add(cat);
                }
            }
        }

        static string ToIdString(XElement src) {
            return src.GetMandatoryAttribute(XNames.xaId, (em) => { throw new BizException(em); });
        }
        #endregion
    }

    /// <summary>
    /// Source filter definition
    /// </summary>
    public class SourceFilter : NotifyPropertyChangedBase, IXmlSource, IXmlConfigurable {
        ///<summary>Folder</summary>
        private string _folder;
        ///<summary>File name filter (e.g. *.jpg)</summary>
        private string _filter;

        ///<summary>File name filter (e.g. *.jpg)</summary>
        public string Filter {
            get { return this._filter; }
            set {
                if (this._filter != value) {
                    this._filter = value;
                    this.FirePropertyChanged("Filter");
                }
            }
        }
        

        ///<summary>Folderr</summary>
        public string Folder {
            get { return this._folder; }
            set {
                if (this._folder != value) {
                    this._folder = value;
                    this.FirePropertyChanged("Filter");
                }
            }
        }
        

        #region XML Configuration
        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <param name="password">The password to be used to encrypt sensitive data. Can be <c>null</c>.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public XElement GetXml(string password = null) {
            XElement xrz = new XElement(XNames.xnFilter, 
                new XAttribute(XNames.xaLocation, (this.Folder??string.Empty).Trim()),
                new XAttribute(XNames.xaPattern, this.Filter));
            return xrz;
        }

        /// <summary>
        /// Applies the configuration.
        /// </summary>
        /// <param name="configSource">The configuration source. Can be <c>null</c>.</param>
        /// <param name="password">The password to be used to decrypt sensitive data. Can be <c>null</c>.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ApplyConfiguration(XElement configSource, string password = null) {
            this.Folder = (configSource.GetMandatoryAttribute(XNames.xaLocation, (em) => { throw new BizException(em); })??string.Empty).Trim();
            this.Filter = configSource.GetMandatoryAttribute(XNames.xaPattern, (em) => { throw new BizException(em); });
        }

        #endregion
    }
}

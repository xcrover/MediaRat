using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {

    /// <summary>
    /// This is project to prepare video content.
    /// </summary>
    public class VideoProject : NotifyPropertyChangedBase, IXmlSource, IXmlConfigurable {
        ///<summary>Title</summary>
        private string _title = "MyProject";
        ///<summary>Description</summary>
        private string _description;
        ///<summary>Project Full file name</summary>
        private string _projectFileName;
        ///<summary>Is dirty</summary>
        private bool _isDirty;
        private int _nextTrackId=1;
        private VideoHelper _vHelper;
        ///<summary>Media Sources</summary>
        private ObservableCollection<MediaSource> _sources= new ObservableCollection<MediaSource>();
        /// <summary>This map contains map from id to IMeduaSource entries and contain both prime MediaSources and Tracks</summary>
        private Dictionary<int, IContentSource> _contentSrcMap = new Dictionary<int, IContentSource>();
        ///<summary>Tracks</summary>
        private ObservableCollection<IMediaTrack> _tracks= new ObservableCollection<IMediaTrack>();
        ///<summary>Prime track group (video root)</summary>
        private MediaTrackGroup _root;

        ///<summary>Prime track group (video root)</summary>
        public MediaTrackGroup Root {
            get { return this._root; }
            set {
                if (this._root != value) {
                    this._root = value;
                    this.FirePropertyChanged("Root");
                }
            }
        }
        

        ///<summary>Tracks</summary>
        public ObservableCollection<IMediaTrack> Tracks {
            get { return this._tracks; }
            protected set { this._tracks = value; }
        }
        

        ///<summary>Media Sources</summary>
        public ObservableCollection<MediaSource> Sources {
            get { return this._sources; }
            protected set { this._sources = value; }
        }
        

 
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

        ///<summary>Is dirty</summary>
        public bool IsDirty {
            get { return this._isDirty; }
            set {
                if (this._isDirty != value) {
                    this._isDirty = value;
                    this.FirePropertyChanged("isDirty");
                }
            }
        }

        /// <summary>
        /// Gets the video helper.
        /// </summary>
        /// <value>
        /// The v helper.
        /// </value>
        public VideoHelper VHelper {
            get { return this._vHelper ?? (this._vHelper = AppContext.Current.GetServiceViaLocator<VideoHelper>()); }
        }

        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoProject"/> class.
        /// </summary>
        public VideoProject() {
            this.PropertyChanged += (s, a) => this.IsDirty = true;
            this.Root = this.CreateNewGroup();
            this.Root.Title = "Root";
            this.Root.Description = "Primary output track of the project";
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
            XElement xdf, xcol, xrz = new XElement(XNames.xnVideoProject,
                new XAttribute(XNames.xaName, this.Title)).AddDescription(this.Description);
            xrz.Add(xdf = new XElement(XNames.xnSourceMedia));
            xdf.AddItems(from ms in this.Sources select ms.GetXml(password));
            xrz.Add(xdf = new XElement(XNames.xnTracks));
            xdf.AddItems(from ms in this.Tracks select ms.GetXml(password));
            xrz.Add(this.Root.GetXml(password));
             // Add source files
            //xrz.Add(xcol = new XElement(XNames.xnMedia));
            //foreach (var cv in this.MediaFiles) {
            //    xcol.Add(cv.GetXml(password));
            //}
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
            this.Title = configSource.GetAttributeValue(XNames.xaName);
            this.Description = configSource.GetDescription();
            xdf = configSource.GetMandatoryElement(XNames.xnSourceMedia, (em) => { throw new BizException(em); });
            MediaSource ms;
            int mxId = int.MinValue;
            foreach(var xms in xdf.Elements(MediaSource.xnMediaSource)) {
                ms = CreateMediaSource(xms);
                mxId = Math.Max(mxId, ms.Id); // have to figure nextId counter
                this.Sources.Add(ms);
                this.RegisterContentSource(ms);
            }
            MediaTrack mt;
            MediaTrackGroup mtg;
            xdf = configSource.Element(XNames.xnTracks);
            if (xdf != null) {
                foreach (var xms in xdf.Elements(MediaTrack.xnTrack)) {
                    mt = CreateMediaTrack(xms);
                    mxId = Math.Max(mxId, mt.Id); // have to figure nextId counter
                    this.Tracks.Add(mt);
                    this.RegisterContentSource(mt);
                }
                foreach (var xms in xdf.Elements(MediaTrackGroup.xnTrackSet)) {
                    mtg = CreateMediaTrackGroup(xms);
                    mxId = Math.Max(mxId, mtg.Id); // have to figure nextId counter
                    this.Tracks.Add(mtg);
                    this.RegisterContentSource(mtg);
                }
            }

            if (null!=(xdf=configSource.Element(MediaTrackGroup.xnTrackSet))) {
                this.Root.ApplyConfiguration(xdf, password);
            }
            //// Reaad media file entries
            //xcol = configSource.GetMandatoryElement(XNames.xnMedia);
            //foreach (var itm in from t in xcol.Elements(XNames.xnItem).Deserialize<MediaFile>(ToMediaFile) orderby t.Title select t) {
            //    this.MediaFiles.Add(itm);
            //}
            this._nextTrackId = mxId + 1;
        }
        #endregion

        #region Operation

        /// <summary>
        /// Gets the next track identifier.
        /// </summary>
        /// <returns></returns>
        public int GetNextTrackId() {
            return Interlocked.Increment(ref this._nextTrackId);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear() {
            this.Title = string.Empty;
            this.Description = string.Empty;
            this.Sources.Clear();
            this._contentSrcMap.Clear();
            this.Tracks.Clear();
         }


        #region Persistence

        /// <summary>
        /// Loads the VideoProject from the specified source path.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <exception cref="Ops.NetCoe.LightFrame.BizException"></exception>
        public static VideoProject Load(string sourcePath, string password=null) {
            try{
                XDocument xd = XDocument.Load(sourcePath);
                VideoProject rz = new VideoProject();
                rz.ApplyConfiguration(xd.Root, password);
                rz.ProjectFileName = sourcePath;
                rz.IsDirty = false;
                return rz;
            }
            catch (Exception x) {
                throw new BizException(string.Format("Failed to load Video Project from {0}. {1}: {2}", sourcePath, x.GetType().Name, x.Message));
            }
        }

        /// <summary>
        /// Saves the specified target path.
        /// </summary>
        /// <param name="targetPath">The target path.</param>
        /// <param name="password">The password.</param>
        /// <exception cref="Ops.NetCoe.LightFrame.BizException"></exception>
        public void Save(string targetPath, string password=null) {
            try {
                XElement xel = this.GetXml(password);
                XDocument xd = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), xel);
                xd.Save(targetPath);
                this.ProjectFileName = targetPath;
                this.IsDirty = false;
            }
            catch (Exception x) {
                throw new BizException(string.Format("Failed to save project to {0}. {1}: {2}", targetPath, x.GetType().Name, x.Message));
            }
        }

        #endregion

        #region Media Sources
        /// <summary>
        /// Starts the creating media sources in async mode.
        /// Retuns list of initialized MediaSources
        /// </summary>
        /// <param name="sourcePathes">The source pathes.</param>
        /// <returns></returns>
        public Task<List<MediaSource>> StartCreatingMediaSources(IEnumerable<string> sourcePathes) {
            this.IsDirty = true; // To avoid further out of thread notifications
            return Task<List<MediaSource>>.Factory.StartNew(() => {
                List<MediaSource> rz = new List<MediaSource>();
                // Existing files
                HashSet<string> existing = new HashSet<string>(
                    from s in this.Sources select s.Source,
                    StringComparer.InvariantCultureIgnoreCase);
                MediaSource nms;
                foreach (var spth in sourcePathes) {
                    if (!existing.Contains(spth)) {
                        nms = CreateMediaSource(spth);
                        if ((nms != null) && (nms.MediaType != MediaTypes.Undefined)) {
                            existing.Add(spth);
                            rz.Add(nms);
                            this.RegisterContentSource(nms);
                        }
                    }
                }
                return rz;
            });
        }

        /// <summary>
        /// Registers the content source.
        /// </summary>
        /// <param name="src">The source.</param>
        void RegisterContentSource(IContentSource src) {
            this._contentSrcMap[src.Id] = src;
        }

        /// <summary>
        /// Adds the media sources and keeps order by title.
        /// MediaSources must be initialized, e.g. created with <see cref="StartCreatingMediaSources"/>
        /// </summary>
        /// <param name="mSources">The m sources.</param>
        public void AddMediaSources(IEnumerable<MediaSource> mSources) {
            var qJ= (from xs in this.Sources select xs).Union(
                from ns in mSources select ns);
            List<MediaSource> tmp = new List<MediaSource>(qJ); // Need some buffer here
            this.Sources.Clear();
            this.Sources.AddItems(from ms in tmp orderby ms.Title select ms);
        }

        MediaSource CreateMediaSource(string sourcePath) {
            MediaSource rz = new MediaSource(this, this.GetNextTrackId());
            rz.Source = sourcePath;
            rz.MediaType = MediaUtil.GetFileMediaType(sourcePath);
            VideoStreamCfg vCfg;
            AudioStreamCfg aCfg;
            if (this.VHelper.TryGetMetadata(sourcePath, out vCfg, out aCfg)) {
                if (vCfg != null) {
                    rz.Height = vCfg.Height;
                    rz.Width = vCfg.Width;
                    rz.DurationS = vCfg.Duration.TotalSeconds;
                }
            }
            return rz;
        }

        /// <summary>
        /// Creates the media source from XML.
        /// </summary>
        /// <param name="xsrc">The XSRC.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        MediaSource CreateMediaSource(XElement xsrc, string password=null) {
            MediaSource rz = new MediaSource(this, -1);
            rz.ApplyConfiguration(xsrc, password);
            return rz;
        }

        /// <summary>
        /// Gets the content source.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public IContentSource GetContentSource(int id) {
            return this._contentSrcMap.GetValueSafe(id);
        }
        #endregion

        #region Tracks

        /// <summary>
        /// Gets the media track.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public IMediaTrack GetMediaTrack(int id) {
            return this._contentSrcMap.GetValueSafe(id) as IMediaTrack;
        }

        /// <summary>
        /// Adds the new track.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public MediaTrack AddNewTrack(IContentSource source) {
            var rz = this.CreateMediaTrack(source);
            this.RegisterContentSource(rz);
            this.Tracks.Add(rz);
            return rz;
        }

        /// <summary>
        /// Adds the new track group.
        /// </summary>
        /// <returns></returns>
        public MediaTrackGroup AddNewTrackGroup(IEnumerable<IMediaTrack> source=null) {
            MediaTrackGroup rz = this.CreateNewGroup(source);
            this.RegisterContentSource(rz);
            this.Tracks.Add(rz);
            return rz;
        }

        MediaTrack CreateMediaTrack(IContentSource src) {
            MediaTrack rz = new MediaTrack(this, this.GetNextTrackId(), src);
            rz.Title = src.Title;
            if (src.MediaType == MediaTypes.Image)
                rz.DurationS = 7;
            else
                rz.DurationS = src.DurationS;
            rz.Description = src.Description;
            return rz;
        }

        MediaTrack CreateMediaTrack(XElement src, string password=null) {
            MediaTrack rz = new MediaTrack(this, 0);
            rz.ApplyConfiguration(src, password);
            return rz;
        }

        MediaTrackGroup CreateMediaTrackGroup(XElement src, string password = null) {
            MediaTrackGroup rz = new MediaTrackGroup(this, MediaTypes.Video, 0);
            rz.ApplyConfiguration(src, password);
            return rz;
        }


        ///// <summary>
        ///// Creates the new track.
        ///// </summary>
        ///// <param name="mediaType">Type of the media.</param>
        ///// <returns></returns>
        ///// <exception cref="System.ApplicationException"></exception>
        //IMediaTrack CreateNewTrack(MediaTypes mediaType) {
        //    switch (mediaType) {
        //        case MediaTypes.Video: return new VideoTrack(this, this.GetNextTrackId());
        //        case MediaTypes.Audio: return new AudioTrack(this, this.GetNextTrackId());
        //        case MediaTypes.Image: return new ImageTrack(this, this.GetNextTrackId());
        //        default: throw new ApplicationException(string.Format("Media type '{0}' not supported", mediaType));
        //    }
        //}

        /// <summary>
        /// Creates the new group.
        /// </summary>
        /// <returns></returns>
        MediaTrackGroup CreateNewGroup(IEnumerable<IMediaTrack> source = null) {
            MediaTrackGroup rz = new MediaTrackGroup(this, MediaTypes.Video, this.GetNextTrackId());
            if (source != null) {
                rz.Tracks.AddItems(source);
            }
            return rz;
        }

        #endregion
        #endregion

    }

}

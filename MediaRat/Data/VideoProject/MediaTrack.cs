using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
    /// <summary>
    /// Media track is an entry on a timeline.
    /// It also can be a content source for other tracks
    /// </summary>
     public class MediaTrack : NotifyPropertyChangedBase, IContentSource, IMediaTrack, IXmlSource, IXmlConfigurable {
        public static XName xnTrack = XName.Get("track");
        ///<summary>Title</summary>
        private string _title;
        ///<summary>Description</summary>
        private string _description;
        ///<summary>Media Source</summary>
        private IContentSource _contentSource;
        ///<summary>Usage time</summary>
        private TrackTime _useTime= new TrackTime();
        ///<summary>Track duration (seconds)</summary>
        private double _durationS;

        ///<summary>Description</summary>
        public string Description {
            get { return this._description ?? ((this.ContentSource == null) ? null : this.ContentSource.Description); }
            set {
                if (this._description != value) {
                    this._description = value;
                    this.FirePropertyChanged("Description");
                }
            }
        }

        ///<summary>Title</summary>
        public string Title {
            get { return this._title??((this.ContentSource==null) ? null : this.ContentSource.Title); }
            set {
                if (this._title != value) {
                    this._title = value;
                    this.FirePropertyChanged("Title");
                }
            }
        }

        ///<summary>Track duration (seconds)</summary>
        public double DurationS {
            get { return this._durationS; }
            set {
                if (this._durationS != value) {
                    this._durationS = value;
                    this.FirePropertyChanged("DurationS");
                }
            }
        }
        

        ///<summary>Usage time</summary>
        public TrackTime UseTime {
            get { return this._useTime; }
            protected set { 
                this._useTime = value;
                CalcDuration();
            }
        }
        
        ///<summary>Media Source</summary>
        public IContentSource ContentSource {
            get { return this._contentSource??(this._contentSource=Project.GetContentSource(this.ContentSourceId)); }
            protected set {
                if (this._contentSource != value) {
                    this._contentSource = value;
                    this.FirePropertyChanged("ContentSource");
                    CalcDuration();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is group of other tracks.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is group; otherwise, <c>false</c>.
        /// </value>
        public bool IsGroup {
            get { return false; }
        }

        /// <summary>
        /// Gets the type of the media.
        /// </summary>
        public MediaTypes MediaType {
            get { return MediaTypes.Video; }
        }

        /// <summary>
        /// Gets the parent project.
        /// </summary>
        public VideoProject Project { get; protected set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public int Id { get; protected set; }
        /// <summary>
        /// Gets or sets the media source identifier.
        /// </summary>
        public int ContentSourceId { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTrack"/> class.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="id">The identifier.</param>
        public MediaTrack(VideoProject project, int id) {
            this.Project = project;
            this.Id = id;
            this.UseTime.PropertyChanged += (s, a) => CalcDuration();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTrack"/> class.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="source">The source.</param>
        public MediaTrack(VideoProject project, int id, IContentSource source) {
            this.Project = project;
            this.Id = id;
            this._contentSource = source;
            if (source != null)
                this.ContentSourceId = source.Id;
            this.UseTime.PropertyChanged += (s, a) => CalcDuration();
            this.CalcDuration();
        }

        void CalcDuration() {
            if (this.ContentSource == null)
                this.DurationS = 0;
            else {
                if (this.ContentSource.MediaType == MediaTypes.Image)
                    this.DurationS= this.UseTime.Stop.HasValue ? this.UseTime.Stop.Value : 0.0;
                else
                    this.DurationS= this.UseTime.GetActualDuration(this.ContentSource.DurationS);
            }
        }

        /// <summary>
        /// After load Duration may be skewed.
        /// This method recalculates what is required.
        /// </summary>
        public void Reset() {
            this.CalcDuration();
        }

        #region IXmlSource Members

        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <param name="password">The password to be used to encrypt sensitive data. Can be <c>null</c>.</param>
        /// <returns></returns>
        public XElement GetXml(string password = null) {
            XElement rz = new XElement(xnTrack,
                new XAttribute(XNames.xaId, this.Id),
                new XAttribute(XNames.xaSrcId, this.ContentSourceId));
            if (this.UseTime.Start.HasValue)
                rz.Add(new XAttribute(XNames.xaStartAt, this.UseTime.Start.Value));
            if (this.UseTime.Stop.HasValue)
                rz.Add(new XAttribute(XNames.xaStopAt, this.UseTime.Stop.Value));
            if (!string.IsNullOrEmpty(this._title))
                rz.Add(new XAttribute(XNames.xaName, this._title));
            rz.AddDescription(this._description);
            return rz;
        }

        #endregion

        #region IXmlConfigurable Members

        /// <summary>
        /// Applies the configuration.
        /// </summary>
        /// <param name="configSource">The configuration source. Can be <c>null</c>.</param>
        /// <param name="password">The password to be used to decrypt sensitive data. Can be <c>null</c>.</param>
        public void ApplyConfiguration(XElement configSource, string password = null) {
            this.Id = configSource.GetMandatoryAttribute(XNames.xaId, (x) => (int)x);
            this.ContentSourceId = configSource.GetMandatoryAttribute(XNames.xaSrcId, (x) => (int)x);
            configSource.TryGetAttribute<int>(XNames.xaStartAt, (x) => { this.UseTime.Start = (double)x; return true; });
            configSource.TryGetAttribute<int>(XNames.xaStopAt, (x) => { this.UseTime.Stop = (double)x; return true; });
            this.Title = configSource.GetAttributeValue(XNames.xaName);
            this.Description = configSource.GetDescription();
            this.ContentSource = this.Project.GetContentSource(this.ContentSourceId);
        }

        #endregion
    }
}

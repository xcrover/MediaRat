using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
    /// <summary>
    /// Group of Media tracks that results in a grouped mediatrack
    /// </summary>
    public class MediaTrackGroup : NotifyPropertyChangedBase, IMediaTrackGroup {
        ///<summary>The XML name for Track Group [trackSet]</summary>
        public static XName xnTrackSet = XName.Get("trackSet");

        ///<summary>Title</summary>
        private string _title;
        ///<summary>Description</summary>
        private string _description;
        ///<summary>Duration [seconds]</summary>
        private double _durationS;

        ///<summary>Duration [seconds]</summary>
        public double DurationS {
            get { return this._durationS; }
            set {
                if (this._durationS != value) {
                    this._durationS = value;
                    this.FirePropertyChanged("DurationS");
                }
            }
        }
        

        /// <summary>
        /// Gets the tracks. This value is never null.
        /// </summary>
        /// <value>
        /// The tracks.
        /// </value>
         public ObservableCollection<IMediaTrack> Tracks { get;  protected set;}
 
        ///<summary>Source</summary>
        public string Source {
            get { return string.Empty; }
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
        

        /// <summary>
        /// Gets the parent project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        public VideoProject Project { get; protected set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; protected set; }
        /// <summary>
        /// Gets the type of the media.
        /// </summary>
        /// <value>
        /// The type of the media.
        /// </value>
        public MediaTypes MediaType { get; protected set; }
        /// <summary>
        /// Gets a value indicating whether this instance is group of other tracks.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is group; otherwise, <c>false</c>.
        /// </value>
        public bool IsGroup { get { return true; } }


        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTrackGroup" /> class.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="mediaType">Type of the media.</param>
        /// <param name="id">The identifier.</param>
        public MediaTrackGroup(VideoProject project, MediaTypes mediaType, int id) {
            this.Id = id;
            this.MediaType = mediaType;
            this.Project = project;
            this.Tracks = new ObservableCollection<IMediaTrack>();
            this.PropertyChanged += (s, a) => this.Project.IsDirty = true;
            this.Tracks.CollectionChanged += Tracks_CollectionChanged;
        }

        void Tracks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            this.Project.IsDirty = true;
            this.DurationS = this.Tracks.Aggregate(0.0, (r, m) => r + m.DurationS);
        }
        #endregion

        #region IXmlSource Members


        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <param name="password">The password to be used to encrypt sensitive data. Can be <c>null</c>.</param>
        /// <returns></returns>
        public XElement GetXml(string password = null) {
            XElement rz = new XElement(xnTrackSet,
                new XAttribute(XNames.xaId, this.Id),
                new XAttribute(XNames.xaType, this.MediaType),
                new XAttribute(XNames.xaName, this.Title));
            if (!string.IsNullOrEmpty(this.Description)) {
                rz.Add(new XElement(XNames.xnDescription, this.Description));
            }
            if (this.Tracks.Count > 0) {
                XElement xits = new XElement(XNames.xnTracks);
                rz.Add(xits);
                for (int i = 0; i < this.Tracks.Count; i++) {
                    xits.Add(new XElement(XNames.xnItem,
                        new XAttribute(XNames.xaTrackId, this.Tracks[i].Id)));
                }
            }
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
            this.Title = configSource.GetAttributeValue(XNames.xaName);
            this.Description = configSource.GetDescription();
            this.MediaType = configSource.GetMandatoryAttribute(XNames.xaType, (x) => x.Value.ToEnum<MediaTypes>(MediaTypes.Undefined));
            XElement xits = configSource.Element(XNames.xnTracks);
            IMediaTrack mt;
            if (xits != null) {
                int trId;
                foreach (var xit in xits.Elements(XNames.xnItem)) {
                    trId = xit.GetMandatoryAttribute<int>(XNames.xaTrackId, (x) => (int)x);
                    if (null != (mt = this.Project.GetMediaTrack(trId)))
                        this.Tracks.Add(mt);
                }
            }
        }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
    /// <summary>
    /// Media source for VideoProject
    /// </summary>
    public class MediaSource : NotifyPropertyChangedBase, IContentSource, IXmlSource, IXmlConfigurable {
        /// <summary>
        /// The xn media source
        /// </summary>
        public static XName xnMediaSource = XName.Get("mediaItem");

        ///<summary>Title</summary>
        private string _title;
         ///<summary>Description</summary>
        private string _description;
        ///<summary>Source</summary>
        private string _source;
        ///<summary>Duration (seconds)</summary>
        private double _durationS;
        ///<summary>Width in pixels</summary>
        private int _width;
        ///<summary>Height in pixels</summary>
        private int _height;

        ///<summary>Resolution Height in pixels</summary>
        public int Height {
            get { return this._height; }
            set {
                if (this._height != value) {
                    this._height = value;
                    this.FirePropertyChanged("Height");
                }
            }
        }
        

        ///<summary>Resolution Width in pixels</summary>
        public int Width {
            get { return this._width; }
            set {
                if (this._width != value) {
                    this._width = value;
                    this.FirePropertyChanged("Width");
                }
            }
        }
        

        ///<summary>Duration (seconds)</summary>
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
        /// Gets the parent project.
        /// </summary>
         public VideoProject Project { get; protected set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public int Id { get; protected set; }
        /// <summary>
        /// Gets the type of the media.
        /// </summary>
        public MediaTypes MediaType { get; set; }

        ///<summary>Source</summary>
        public string Source {
            get { return this._source; }
            set {
                if (this._source != value) {
                    this._source = value;
                    this.FirePropertyChanged("Source");
                    if (!string.IsNullOrEmpty(value))
                        this.Title = System.IO.Path.GetFileNameWithoutExtension(value);
                    else {
                        this.Title = string.Empty;
                    }
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaSource"/> class.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="id">The identifier.</param>
        public MediaSource(VideoProject project, int id) {
            this.Id = id;
            this.Project = project;
            this.PropertyChanged += (s, a) => this.Project.IsDirty = true;
        }

        #region IXmlSource Members

        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <param name="password">The password to be used to encrypt sensitive data. Can be <c>null</c>.</param>
        /// <returns></returns>
        public XElement GetXml(string password = null) {
            XElement rz = new XElement(xnMediaSource,
                new XAttribute(XNames.xaId, this.Id),
                new XAttribute(XNames.xaType, this.MediaType),
                new XAttribute(XNames.xaLocation, this.Source));
            switch (this.MediaType) {
                case MediaTypes.Video:
                    rz.Add(new XAttribute(XNames.xaWidth, this.Width),
                        new XAttribute(XNames.xaHeight, this.Height),
                        new XAttribute(XNames.xaDuration, this.DurationS));
                    break;
                case MediaTypes.Audio:
                    rz.Add(new XAttribute(XNames.xaDuration, this.DurationS));
                    break;
                case MediaTypes.Image:
                    rz.Add(new XAttribute(XNames.xaWidth, this.Width),
                        new XAttribute(XNames.xaHeight, this.Height));
                    break;
            }
            if (!string.IsNullOrEmpty(this.Description)) {
                rz.Add(new XElement(XNames.xnDescription, this.Description));
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
            this.MediaType = configSource.GetMandatoryAttribute(XNames.xaType, (x) => x.Value.ToEnum<MediaTypes>(MediaTypes.Undefined));
            configSource.TryGetAttribute<int>(XNames.xaWidth, (x) => { this.Width = (int)x; return true; });
            configSource.TryGetAttribute<int>(XNames.xaHeight, (x) => { this.Height = (int)x; return true; });
            configSource.TryGetAttribute<double>(XNames.xaDuration, (x) => { this.DurationS = (double)x; return true; });
            this.Source = configSource.GetAttributeValue(XNames.xaLocation);
            this.Description = configSource.GetDescription();
        }

        #endregion

    }
}

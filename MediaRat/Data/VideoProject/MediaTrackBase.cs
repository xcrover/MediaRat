using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
    ///// <summary>
    ///// Base class for different mediatracks
    ///// </summary>
    //abstract public class MediaTrackBase : NotifyPropertyChangedBase, IMediaTrack {
    //    ///<summary>Title</summary>
    //    private string _title;
    //    ///<summary>Description</summary>
    //    private string _description;
    //    ///<summary>Source</summary>
    //    private string _source;

    //    ///<summary>Source</summary>
    //    public string Source {
    //        get { return this._source; }
    //        set {
    //            if (this._source != value) {
    //                this._source = value;
    //                this.FirePropertyChanged("Source");
    //            }
    //        }
    //    }
        

    //    ///<summary>Description</summary>
    //    public string Description {
    //        get { return this._description; }
    //        set {
    //            if (this._description != value) {
    //                this._description = value;
    //                this.FirePropertyChanged("Description");
    //            }
    //        }
    //    }
        

    //    ///<summary>Title</summary>
    //    public string Title {
    //        get { return this._title; }
    //        set {
    //            if (this._title != value) {
    //                this._title = value;
    //                this.FirePropertyChanged("Title");
    //            }
    //        }
    //    }
        

    //    /// <summary>
    //    /// Gets the parent project.
    //    /// </summary>
    //    /// <value>
    //    /// The project.
    //    /// </value>
    //    public VideoProject Project { get; protected set; }

    //    /// <summary>
    //    /// Gets the identifier.
    //    /// </summary>
    //    /// <value>
    //    /// The identifier.
    //    /// </value>
    //    public int Id { get; protected set; }
    //    /// <summary>
    //    /// Gets the type of the media.
    //    /// </summary>
    //    /// <value>
    //    /// The type of the media.
    //    /// </value>
    //    public MediaTypes MediaType { get; protected set; }
    //    /// <summary>
    //    /// Gets a value indicating whether this instance is group of other tracks.
    //    /// </summary>
    //    /// <value>
    //    ///   <c>true</c> if this instance is group; otherwise, <c>false</c>.
    //    /// </value>
    //    virtual public bool IsGroup { get { return false; } }


    //    #region Construction
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="MediaTrackBase" /> class.
    //    /// </summary>
    //    /// <param name="project">The project.</param>
    //    /// <param name="mediaType">Type of the media.</param>
    //    /// <param name="id">The identifier.</param>
    //    public MediaTrackBase(VideoProject project, MediaTypes mediaType, int id) {
    //        this.Id = id;
    //        this.MediaType = mediaType;
    //        this.Project = project;
    //        this.PropertyChanged += (s, a) => this.Project.IsDirty = true;
    //    }
    //    #endregion

    //    #region IXmlSource Members

    //    /// <summary>
    //    /// Gets the XML name of this track.
    //    /// </summary>
    //    /// <returns></returns>
    //    virtual public XName GetXName() {
    //        return XNames.xnItem;
    //    }

    //    public XElement GetXml(string password = null) {
    //        XElement rz = new XElement(this.GetXName(),
    //            new XAttribute(XNames.xaId, this.Id),
    //            new XAttribute(XNames.xaType, this.MediaType),
    //            new XAttribute(XNames.xaName, this.Title),
    //            new XAttribute(XNames.xaLocation, this.Source));
    //        if (!string.IsNullOrEmpty(this.Description)) {
    //            rz.Add(new XElement(XNames.xnDescription, this.Description));
    //        }
    //        return rz;
    //    }

    //    #endregion

    //    #region IXmlConfigurable Members

    //    /// <summary>
    //    /// Applies the configuration.
    //    /// </summary>
    //    /// <param name="configSource">The configuration source. Can be <c>null</c>.</param>
    //    /// <param name="password">The password to be used to decrypt sensitive data. Can be <c>null</c>.</param>
    //    public void ApplyConfiguration(XElement configSource, string password = null) {
    //        this.Title = configSource.GetAttributeValue(XNames.xaName);
    //        this.Source = configSource.GetAttributeValue(XNames.xaLocation);
    //        this.Description = configSource.GetDescription();
    //    }

    //    #endregion
    //}
}

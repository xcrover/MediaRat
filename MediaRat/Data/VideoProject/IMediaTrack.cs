using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
    /// <summary>
    /// Media Track (as Part of VideoProject) contract
    /// </summary>
    public interface IMediaTrack : INotifyPropertyChanged, IContentSource, IXmlSource, IXmlConfigurable {
        /// <summary>
        /// Gets the parent project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        VideoProject Project { get; }
        ///// <summary>
        ///// Gets the identifier.
        ///// </summary>
        ///// <value>
        ///// The identifier.
        ///// </value>
        //int Id { get; }
        ///// <summary>
        ///// Gets the type of the media.
        ///// </summary>
        ///// <value>
        ///// The type of the media.
        ///// </value>
        //MediaTypes MediaType { get; }
        /// <summary>
        /// Gets a value indicating whether this instance is group of other tracks.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is group; otherwise, <c>false</c>.
        /// </value>
        bool IsGroup { get; }
        ///// <summary>
        ///// Gets the title.
        ///// </summary>
        ///// <value>
        ///// The title.
        ///// </value>
        //string Title { get; }
        ///// <summary>
        ///// Gets the description.
        ///// </summary>
        ///// <value>
        ///// The description.
        ///// </value>
        //string Description { get; }
        ///// <summary>
        ///// Gets the source.
        ///// </summary>
        ///// <value>
        ///// The source.
        ///// </value>
        //string Source { get; }
    }

    /// <summary>
    /// Collection of tracks
    /// </summary>
    public interface IMediaTrackGroup : IMediaTrack {
        /// <summary>
        /// Gets the tracks. This value is never null.
        /// </summary>
        /// <value>
        /// The tracks.
        /// </value>
        ObservableCollection<IMediaTrack> Tracks { get; }

    }
}

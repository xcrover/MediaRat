using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XC.MediaRat {
    /// <summary>
    /// Media Source contract
    /// </summary>
    public interface IContentSource {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        int Id { get; }
        /// <summary>
        /// Gets the type of the media.
        /// </summary>
        MediaTypes MediaType { get; }
        /// <summary>
        /// Gets the duration s.
        /// </summary>
        double DurationS { get; }
        /// <summary>
        /// Gets the title.
        /// </summary>
        string Title { get; }
        /// <summary>
        /// Gets the description.
        /// </summary>
        string Description { get; }

    }
}

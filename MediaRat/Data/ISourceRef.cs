using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace XC.MediaRat {

    /// <summary>
    /// Just a reference to the source media file with information that can be obtained from the file name
    /// </summary>
    public interface ISourceRef {
        MediaTypes MediaType { get; }

        string SourcePath { get; }
    }

    /// <summary>
    /// Class that has current source
    /// </summary>
    public interface ISourceProvider : INotifyPropertyChanged {
        ISourceRef ActiveSource { get; }
    }
}

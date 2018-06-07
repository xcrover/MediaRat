using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
    /// <summary>
    /// Image data
    /// </summary>
    public class ImageData : NotifyPropertyChangedBase {
        ///<summary>Path</summary>
        private string _fileName;
        ///<summary>Media Properties</summary>
        private KeyValuePairXCol<string, string> _mediaProps;
        ///<summary>Current Image</summary>
        private ImageSource _content;


        ///<summary>Media Properties</summary>
        public KeyValuePairXCol<string, string> MediaProps {
            get { return this._mediaProps; }
            set {
                if (this._mediaProps != value) {
                    this._mediaProps = value;
                    this.FirePropertyChanged("MediaProps");
                }
            }
        }

        ///<summary>Path</summary>
        public string FileName {
            get { return this._fileName; }
            set {
                if (this._fileName != value) {
                    this._fileName = value;
                    this.FirePropertyChanged("FileName");
                }
            }
        }

        ///<summary>Current Image</summary>
        public ImageSource Content {
            get { return this._content; }
            set {
                if (this._content != value) {
                    this._content = value;
                    this.FirePropertyChanged("Content");
                }
            }
        }

    }

    public class ImageDim {
        public uint Width { get; set; }
        public uint Height { get; set; }

        public bool IsVert {
            get {
                return this.Height > this.Width;
            }
        }

        public override string ToString() {
            return string.Format("{0}x{1} px, {2}", Width, Height, IsVert ? "Vertical" : "Horizontal");
        }
    }
}

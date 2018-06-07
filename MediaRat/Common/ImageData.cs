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

        /// <summary>
        /// Set Width and Height to 0
        /// </summary>
        public void Clear() {
            this.Height = this.Width = 0;
        }

        /// <summary>
        /// Set values from the <paramref name="source"/>. If <paramref name="source"/> is null then <see cref="Clear"/>.
        /// </summary>
        /// <param name="source"></param>
        public void Set(ImageDim source) {
            if (source == null) {
                this.Clear();
            }
            else {
                this.Width = source.Width;
                this.Height = source.Height;
            }
        }

        /// <summary>
        /// True if at one of the dimensions equals 0
        /// </summary>
        public bool IsEmpty {
            get { return (this.Width == 0) || (this.Height == 0); }
        }

        public override string ToString() {
            return string.Format("{0}x{1} px, {2}", Width, Height, IsVert ? "Vertical" : "Horizontal");
        }

        public string ToShortStr() {
            return string.Format("{0}x{1} [{2}]", Width, Height, IsVert ? "V" : "H");
        }
    }
}

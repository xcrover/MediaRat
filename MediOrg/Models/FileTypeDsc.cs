using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ops.NetCoe.LightFrame;

namespace MediOrg.Models {
    public class FileTypeDsc : NotifyPropertyChangedBase {
        ///<summary>Title</summary>
        private string _title;
        ///<summary>File extension</summary>
        private string _fileExt;
        ///<summary>Is Marked</summary>
        private bool _isMarked;
        public int Count { get; set; }

        ///<summary>Is Marked</summary>
        public bool IsMarked {
            get { return this._isMarked; }
            set {
                if (this._isMarked != value) {
                    this._isMarked = value;
                    this.FirePropertyChanged(nameof(IsMarked));
                }
            }
        }

        ///<summary>File extension</summary>
        public string FileExt {
            get { return this._fileExt; }
            set {
                if (this._fileExt != value) {
                    this._fileExt = value;
                    this.FirePropertyChanged(nameof(FileExt));
                }
            }
        }

        ///<summary>Title</summary>
        public string Title {
            get { return this._title; }
            set {
                if (this._title != value) {
                    this._title = value;
                    this.FirePropertyChanged(nameof(Title));
                }
            }
        }

    }
}

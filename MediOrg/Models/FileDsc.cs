using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ops.NetCoe.LightFrame;

namespace MediOrg.Models {
    public class FileDsc : NotifyPropertyChangedBase  {
        ///<summary>Name</summary>
        private string _name;
        ///<summary>File time</summary>
        private DateTime _fileTime;
        ///<summary>File extension</summary>
        private string _extension;
        ///<summary>Name without extension</summary>
        private string _title;
        ///<summary>New title</summary>
        private string _newTitle;
        public FileGroupDsc Group { get; set; }

        ///<summary>New title</summary>
        public string NewTitle {
            get { return this._newTitle; }
            set {
                if (this._newTitle != value) {
                    this._newTitle = value;
                    this.FirePropertyChanged(nameof(NewTitle));
                }
            }
        }


        ///<summary>Name without extension</summary>
        public string Title {
            get { return this._title; }
            set {
                if (this._title != value) {
                    this._title = value;
                    this.FirePropertyChanged(nameof(Title));
                }
            }
        }


        ///<summary>File extension</summary>
        public string Extension {
            get { return this._extension; }
            set {
                if (this._extension != value) {
                    this._extension = value;
                    this.FirePropertyChanged(nameof(Extension));
                }
            }
        }


        ///<summary>File time</summary>
        public DateTime FileTime {
            get { return this._fileTime; }
            set {
                if (this._fileTime != value) {
                    this._fileTime = value;
                    this.FirePropertyChanged(nameof(FileTime));
                }
            }
        }


        ///<summary>Name</summary>
        public string Name {
            get { return this._name; }
            set {
                if (this._name != value) {
                    this._name = value;
                    this.FirePropertyChanged(nameof(Name));
                }
            }
        }

    }
}

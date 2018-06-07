using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ops.NetCoe.LightFrame;

namespace MediOrg.Models {
    public class FileNameTransDsc : NotifyPropertyChangedBase {
        ///<summary>Current Name</summary>
        private string _currentName;
        ///<summary>New name</summary>
        private string _newName;
        ///<summary>File time</summary>
        private DateTime _fileTime;

        ///<summary>File time</summary>
        public DateTime fileTime {
            get { return this._fileTime; }
            set {
                if (this._fileTime != value) {
                    this._fileTime = value;
                    this.FirePropertyChanged(nameof(fileTime));
                }
            }
        }


        ///<summary>New name</summary>
        public string NewName {
            get { return this._newName; }
            set {
                if (this._newName != value) {
                    this._newName = value;
                    this.FirePropertyChanged(nameof(NewName));
                }
            }
        }


        ///<summary>Current Name (no extension)</summary>
        public string CurrentName {
            get { return this._currentName; }
            set {
                if (this._currentName != value) {
                    this._currentName = value;
                    this.FirePropertyChanged(nameof(CurrentName));
                }
            }
        }

    }
}

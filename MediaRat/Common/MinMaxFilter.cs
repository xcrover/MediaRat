using Ops.NetCoe.LightFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XC.MediaRat {
    public class MinMaxFilter<T> : NotifyPropertyChangedBase where T : struct, IComparable<T> {
        ///<summary>Label</summary>
        private string _label;
        ///<summary>Description</summary>
        private string _description;
        ///<summary>UI Hint Code</summary>
        private string _uiHCode;
        ///<summary>Min Value</summary>
        private T? _minVal;
        ///<summary>Max Value</summary>
        private T? _maxVal;

        ///<summary>Max Value</summary>
        public T? MaxVal {
            get { return this._maxVal; }
            set {
                if (!object.Equals(this._maxVal, value)) {
                    this._maxVal = value;
                    this.FirePropertyChanged("MaxVal");
                }
            }
        }


        ///<summary>Min Value</summary>
        public T? MinVal {
            get { return this._minVal; }
            set {
                if (!object.Equals(this._minVal, value)) {
                    this._minVal = value;
                    this.FirePropertyChanged("MinVal");
                }
            }
        }


        ///<summary>UI Hint Code</summary>
        public string UiHCode {
            get { return this._uiHCode; }
            set {
                if (this._uiHCode != value) {
                    this._uiHCode = value;
                    this.FirePropertyChanged("UiHCode");
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


        ///<summary>Label</summary>
        public string Label {
            get { return this._label; }
            set {
                if (this._label != value) {
                    this._label = value;
                    this.FirePropertyChanged("Label");
                }
            }
        }

        public bool IsEmpty {
            get { return !(this.MinVal.HasValue || this.MaxVal.HasValue); }
        }

        public bool IsMatch(T val) {
            if ((this.MinVal.HasValue) && (this.MinVal.Value.CompareTo(val) > 0))
                return false;
            if ((this.MaxVal.HasValue) && (this.MaxVal.Value.CompareTo(val) < 0))
                return false;
            return true;
        }


    }
}

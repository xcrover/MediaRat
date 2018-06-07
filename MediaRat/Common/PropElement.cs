using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {

    /// <summary>
    /// Class that represents a single media file property (e.g. Rating or category) with value and
    /// bool marker.
    /// It is used for group updates
    /// </summary>
     public class PropElement : NotifyPropertyChangedBase {
        ///<summary>Is marked</summary>
        private bool _isMarked;
        ///<summary>Title</summary>
        private string _title;
        ///<summary>Vale text</summary>
        private string _valueText;
        ///<summary>Tag with real value</summary>
        private object _tag;
        ///<summary>Apply to target delegate</summary>
        private Action<PropElement,MediaFile> _applicator;
        ///<summary>Prop type reference (e.g. RatingDefinition)</summary>
        private object _typeRef;

        ///<summary>Prop type reference (e.g. RatingDefinition)</summary>
        public object TypeRef {
            get { return this._typeRef; }
            set { this._typeRef = value; }
        }
        

        ///<summary>Apply to target delegate</summary>
        public Action<PropElement,MediaFile> Applicator {
            get { return this._applicator; }
            set { this._applicator = value; }
        }
        

        ///<summary>Tag with real value</summary>
        public object Tag {
            get { return this._tag; }
            set {
                if (this._tag != value) {
                    this._tag = value;
                    this.FirePropertyChanged("Tag");
                }
            }
        }
        

        ///<summary>Vale text</summary>
        public string ValueText {
            get { return this._valueText; }
            set {
                if (this._valueText != value) {
                    this._valueText = value;
                    this.FirePropertyChanged("ValueText");
                }
            }
        }
        

        ///<summary>Title</summary>
        public string Title {
            get { return this._title; }
            set {
                if (this._title != value) {
                    this._title = value;
                    this.FirePropertyChanged("Title");
                }
            }
        }
        

        ///<summary>Is marked</summary>
        public bool IsMarked {
            get { return this._isMarked; }
            set {
                if (this._isMarked != value) {
                    this._isMarked = value;
                    this.FirePropertyChanged("IsMarked");
                }
            }
        }
        
    }
}

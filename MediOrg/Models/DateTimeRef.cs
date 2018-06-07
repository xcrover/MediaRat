using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ops.NetCoe.LightFrame;

namespace MediOrg.Models {
    public class DateTimeRef : NotifyPropertyChangedBase  {
        ///<summary>Value</summary>
        private DateTime _value;

        public DateTimeRef() {}

        public DateTimeRef(DateTime val) {
            this._value = val;
        }

        ///<summary>Value</summary>
        public DateTime Value {
            get { return this._value; }
            set {
                if (this._value != value) {
                    this._value = value;
                    this.FirePropertyChanged(nameof(value));
                }
            }
        }

    }
}

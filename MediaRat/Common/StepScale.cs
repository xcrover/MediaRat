using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
    /// <summary>
    /// Class that emulates stepped scale
    /// </summary>
    public class StepScale : NotifyPropertyChangedBase {
        ///<summary>Max Step index</summary>
        private int _maxP;
        ///<summary>Current step index</summary>
        private int _currentP;
        ///<summary>Current value</summary>
        private double _value;
        ///<summary>Step values</summary>
        private double[] _steps;
        ///<summary>Default position index</summary>
        private int _defaultP;

        ///<summary>Step values</summary>
        public double[] Steps {
            get { return this._steps; }
            protected set { this._steps = value; }
        }

        ///<summary>Current value</summary>
        public double Value {
            get { return this._value; }
            protected set {
                if (this._value != value) {
                    this._value = value;
                    this.FirePropertyChanged("Value");
                }
            }
        }
        

        ///<summary>Current step index</summary>
        public int CurrentP {
            get { return this._currentP; }
            set {
                if (this._currentP != value) {
                    if ((value<0)||(value>=this.Steps.Length)) {
                        throw new ArgumentException(string.Format("Specified Step scale current position {0} must be in range [0..{1}]", value, this.Steps.Length-1), "CurrentP");
                    }
                    this._currentP = value;
                    this.Value = this.Steps[value];
                    this.FirePropertyChanged("CurrentP");
                }
            }
        }
        

        ///<summary>Max Step index</summary>
        public int MaxP {
            get { return this._maxP; }
            protected set {
                if (this._maxP != value) {
                    this._maxP = value;
                    this.FirePropertyChanged("MaxP");
                }
            }
        }

        ///<summary>Default position index</summary>
        public int DefaultP {
            get { return this._defaultP; }
            set { this._defaultP = value; }
        }


        /// <summary>
        /// Initializes the specified step count.
        /// </summary>
        /// <param name="stepCount">The step count.</param>
        /// <param name="baseValue">The base value.</param>
        /// <param name="stepK">The step k.</param>
        public void Init(int stepCount, double baseValue, double stepK) {
            int midP = stepCount / 2;
            this.MaxP = stepCount - 1;
            this._steps = new double[stepCount];
            this._steps[midP] = baseValue;
            for (int i = midP - 1; i >= 0; i--) {
                this._steps[i] = this._steps[i + 1] / stepK;
            }
            for (int i = midP + 1; i<this._steps.Length; i++) {
                this._steps[i] = this._steps[i - 1] * stepK;
            }
            this.CurrentP = this._defaultP= midP;
        }

        /// <summary>
        /// Set default position
        /// </summary>
        public void SetDefaultPos() {
            this.CurrentP = this.DefaultP;
        }
    }
}

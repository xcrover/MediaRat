using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
    /// <summary>
    /// Track time span
    /// </summary>
    public class TrackTime : NotifyPropertyChangedBase {
        ///<summary>Start time (sec)</summary>
        private double? _start;
        ///<summary> (sec)</summary>
        private double? _stop;

        ///<summary> (sec)</summary>
        public double? Stop {
            get { return this._stop; }
            set {
                if (this._stop != value) {
                    this._stop = value;
                    this.FirePropertyChanged("Stop");
                }
            }
        }
        

        ///<summary>Start time (sec)</summary>
        public double? Start {
            get { return this._start; }
            set {
                if (this._start != value) {
                    this._start = value;
                    this.FirePropertyChanged("Start");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid {
            get {
                if (!this.Start.HasValue || !this.Stop.HasValue)
                    return true;
                return this.Start.Value <= this.Stop.Value;
            }
        }

        /// <summary>
        /// Gets the actual duration.
        /// </summary>
        /// <param name="srcDuration">Duration of the source.</param>
        /// <returns></returns>
        public double GetActualDuration(double srcDuration) {
            if (!IsValid) return 0;
            double cd = srcDuration;
            if (Stop.HasValue) {
                if (cd > Stop.Value)
                    cd = Stop.Value;
            }
            if (Start.HasValue) {
                if (Start.Value<cd)
                    cd-= Start.Value;
                else
                    cd=0;
            }
            return cd;
        }
    }

    /// <summary>
    /// Track time based on TimeSpan
    /// </summary>
    public class TrackTimeT : NotifyPropertyChangedBase {
        ///<summary>Start time (sec)</summary>
        private TimeSpan? _start;
        ///<summary> (sec)</summary>
        private TimeSpan? _stop;

        ///<summary> (sec)</summary>
        public TimeSpan? Stop {
            get { return this._stop; }
            set {
                if (this._stop != value) {
                    this._stop = value;
                    this.FirePropertyChanged("Stop");
                    this.FirePropertyChanged("Duration");
                }
            }
        }


        ///<summary>Start time (sec)</summary>
        public TimeSpan? Start {
            get { return this._start; }
            set {
                if (this._start != value) {
                    this._start = value;
                    this.FirePropertyChanged("Start");
                    this.FirePropertyChanged("Duration");
                }
            }
        }


        public TimeSpan? Duration {
            get {
                if (Stop.HasValue) {
                    if (Start.HasValue) {
                        return Stop.Value - Start.Value;
                    }
                    else {
                        return Stop;
                    }
                }
                else {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid {
            get {
                if (!this.Start.HasValue || !this.Stop.HasValue)
                    return true;
                return this.Start.Value <= this.Stop.Value;
            }
        }

        /// <summary>
        /// Gets the actual duration.
        /// </summary>
        /// <param name="srcDuration">Duration of the source.</param>
        /// <returns></returns>
        public double GetActualDuration(double srcDuration) {
            if (!IsValid) return 0;
            double dt, cd = srcDuration;
            if (Stop.HasValue) {
                dt = Stop.Value.TotalSeconds;
                if (cd > dt)
                    cd = dt;
            }
            if (Start.HasValue) {
                dt = Start.Value.TotalSeconds;
                if (dt < cd)
                    cd -= dt;
                else
                    cd = 0;
            }
            return cd;
        }
    }

}

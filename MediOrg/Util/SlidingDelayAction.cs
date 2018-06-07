using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ops.NetCoe.LightFrame {
    public class SlidingDelayAction : IDisposable {
        private System.Threading.Timer _wtimer;
        /// <summary>
        /// Milliseconds to wait since last slide to try trigger the action
        /// </summary>
        public double WaitMs { get; protected set; }
        /// <summary>
        /// Action to execute
        /// </summary>
        public Action Worker { get; protected set; }
        ///<summary>Next planned execution time in binary form</summary>
        private long _nextPlanedTime;

        ///<summary>Next planned execution time</summary>
        public DateTime NextPlanedTime {
            get { return DateTime.FromBinary(this._nextPlanedTime); }
            //set { this._nextPlanedTime = value; }
        }

        /// <summary>
        /// Create sliding action.
        /// Each call of <see cref="Slide"/> restarts action timer.
        /// Time will fire once and wait untill next <see cref="Slide"/> reschedule the worker.
        /// </summary>
        /// <param name="waitMs">Wait time after <see cref="Slide"/></param>
        /// <param name="worker">Action to execute.</param>
        public SlidingDelayAction(double waitMs, Action worker) {
            this.WaitMs = waitMs;
            this.Worker = worker;
        }

        void RunWorker(object o) {
            Worker();
        }

        public void Slide() {
            Interlocked.Exchange(ref this._nextPlanedTime, DateTime.Now.AddMilliseconds(WaitMs).ToBinary());
            if (_wtimer!=null) {
                _wtimer.Change((long)WaitMs, Timeout.Infinite);
            }
            else {
                _wtimer = new Timer(RunWorker, null, (long)WaitMs, Timeout.Infinite);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    if (this._wtimer != null) {
                        this._wtimer.Dispose();
                        this._wtimer = null;
                    }
                }
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SlidingDelayAction() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

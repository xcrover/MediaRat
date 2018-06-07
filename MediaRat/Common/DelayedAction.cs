using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ops.NetCoe.LightFrame {

    /// <summary>
    /// Delayed operation
    /// </summary>
    public class DelayedAction<T> {
        ///<summary>Action parameter</summary>
        private T _parameter;
        ///<summary>Delay milliseconds</summary>
        private long _delay;
        ///<summary>Action to execute after delay</summary>
        private Action<T> _worker;

        ///<summary>Action to execute after delay</summary>
        public Action<T> Worker {
            get { return this._worker; }
            set { this._worker = value; }
        }

        ///<summary>Delay milliseconds</summary>
        public long Delay {
            get { return this._delay; }
            set { this._delay = value; }
        }


        ///<summary>Action parameter</summary>
        public T Parameter {
            get { return this._parameter; }
            set { this._parameter = value; }
        }

        /// <summary>
        /// Wait for <see cref="Delay"/> and then execute the <see cref="Worker"/> with <see cref="Parameter"/>.
        /// </summary>
        public void WaitAndExecute() {
            var timer = new System.Threading.Timer((o) => {
                DelayedAction<T> act = (DelayedAction<T>)o;
                act.Worker(act.Parameter);
            }, this, this.Delay, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Starts the specified action with paramter after the specified delay.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <param name="worker">The worker.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public static DelayedAction<T> Start(long delay, Action<T> worker, T parameter = default(T)) {
            DelayedAction<T> dact = new DelayedAction<T>() {
                Delay = delay,
                Parameter = parameter,
                Worker = worker
            };
            dact.WaitAndExecute();
            return dact;
        }

    }
}

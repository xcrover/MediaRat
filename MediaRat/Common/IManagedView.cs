using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
    /// <summary>
    /// Managed view
    /// </summary>
    public interface IManagedView : IBaseView {

        /// <summary>
        /// Ensures the visible.
        /// </summary>
        void EnsureVisible();

        /// <summary>
        /// Closes this instance.
        /// </summary>
        void Close();

    }
}

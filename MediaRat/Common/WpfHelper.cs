using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace XC.MediaRat {

    /// <summary>
    /// WPF helper extensions
    /// </summary>
    public static class WpfExtensions {

        /// <summary>Finds the TV item.</summary>
        /// <param name="source">The source.</param>
        /// <param name="dtn">The DTN.</param>
        /// <returns></returns>
        public static TreeViewItem FindTVItem(this TreeView source, IDataTreeNode dtn) {
            if (source.HasItems && dtn != null) {
                TreeViewItem tvi = null;
                ItemContainerGenerator icg = source.ItemContainerGenerator;
                foreach (var d in dtn.EnumerateFromTop()) {
                    tvi = icg.ContainerFromItem(d) as TreeViewItem;
                    if (tvi == null)
                        return null;
                    icg = tvi.ItemContainerGenerator;
                }
                return tvi;
            }
            else
                return null;
        }


    }
}

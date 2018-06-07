using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat.Views {
    /// <summary>
    /// Interaction logic for MeidaRenameView.xaml
    /// </summary>
    public partial class MeidaRenameView : Window, IBaseView {
        public MeidaRenameView() {
            InitializeComponent();
        }

        #region IBaseView Members

        /// <summary>
        /// Sets the view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void SetViewModel(ViewModelBase viewModel) {
            this.DataContext = viewModel;
            WorkspaceViewModel wvm = viewModel as WorkspaceViewModel;
            if (wvm != null) {
                wvm.RequestClose += wvm_RequestClose;
            }
        }

        void wvm_RequestClose(object sender, EventArgs e) {
            this.Close();
        }

        #endregion

        /// <summary>
        /// Handles the Closed event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Window_Closed(object sender, EventArgs e) {
            ViewModelBase vm = this.DataContext as ViewModelBase;
            if (vm != null)
                vm.Dispose();
        }
    }
}

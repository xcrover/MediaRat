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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat.Views {
    /// <summary>
    /// Interaction logic for PropElementListView.xaml
    /// </summary>
    public partial class PropElementListView : UserControl, IBaseView {
        public PropElementListView() {
            InitializeComponent();
        }

        #region IBaseView Members

        /// <summary>
        /// Sets the view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void SetViewModel(ViewModelBase viewModel) {
            this._view.DataContext = viewModel;
        }

        #endregion
    }
}

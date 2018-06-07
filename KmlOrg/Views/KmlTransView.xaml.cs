using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KmlOrg.Models;
using Ops.NetCoe.LightFrame;

namespace KmlOrg.Views {
    /// <summary>
    /// Interaction logic for KmlTransView.xaml
    /// </summary>
    public partial class KmlTransView : UserControl, IBaseView {
        public KmlTransView() {
            InitializeComponent();
        }

        public KmlTransVModel ViewModel {
            get { return this._view.DataContext as KmlTransVModel; }
            set { this._view.DataContext = value; }
        }

        public void SetViewModel(ViewModelBase viewModel) {
            this._view.DataContext = viewModel;
        }
    }
}

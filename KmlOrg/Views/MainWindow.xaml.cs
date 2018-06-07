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
using Ops.NetCoe.LightFrame;

namespace KmlOrg {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            Bootstrap.EnsureContext();
            this._klmTrans.ViewModel = new Models.KmlTransVModel();
            this.Title = string.Format("{0} {1}", AppContext.Current.ApplicationTitle, this.GetType().Assembly.GetName().Version);
        }

        public IMessagePresenter GetActiveMessagePresenter() {
            return this._klmTrans.ViewModel.Status;
        }

    }
}

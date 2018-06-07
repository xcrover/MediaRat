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
using MediOrg.Models;
using Ops.NetCoe.LightFrame;

namespace MediOrg.Views {
    /// <summary>
    /// Interaction logic for MediaRenameView.xaml
    /// </summary>
    public partial class MediaRenameView : UserControl, IBaseView {
        public MediaRenameView() {
            InitializeComponent();
            this._cntFmt.Items.Add("0");
            this._cntFmt.Items.Add("00");
            this._cntFmt.Items.Add("000");
            this._cntFmt.Items.Add("0000");
            this._cntFmt.Items.Add("00000");
            this._cntFmt.Items.Add("000000");
        }

        public MediaRenameVModel ViewModel {
            get { return this._view.DataContext as MediaRenameVModel; }
            set { this._view.DataContext = value; }
        }


        public void SetViewModel(ViewModelBase viewModel) {
            ViewModel = viewModel as MediaRenameVModel;
        }

        private void _folder_Drop(object sender, DragEventArgs e) {
            var vm = this.ViewModel;
            if (vm != null) {
                if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files.Length>0) {
                        vm.CurrentFolder = files[0];
                        vm.Refresh();
                    }
                    //vm.ProcessDroppedFiles(files);
                }
            }
        }

        private void _interval_MouseWheel(object sender, MouseWheelEventArgs e) {
            var vm = this.ViewModel;
            if (vm != null)
                vm.AddIntervalUnits((e.Delta>0) ? 1 : -1);
        }
 
        private void _picPreview_MouseDown(object sender, MouseButtonEventArgs e) {
            if ((e.ChangedButton==MouseButton.Left)&&(e.ClickCount==2)) {
                var vm = this.ViewModel;
                if (vm != null)
                    vm.RunDefaultActionOnCurrentMedia();
            }
        }
    }
}

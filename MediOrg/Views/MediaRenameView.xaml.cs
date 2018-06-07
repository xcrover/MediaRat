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

        /// <summary>
        /// When user double clicks on group navigate to the first file in this group
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _groups_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (this._groups.SelectedItem == null) return;
            var vm = this.ViewModel;
            if (vm != null) {
                var grp = this._groups.SelectedItem as FileGroupDsc;
                // Try to find first image or another file if there is no image and make it curent
                var fileToShow = vm.FindFirstImageForTheGroup(grp) ?? vm.FindFirstFileForTheGroup(grp);
                if (fileToShow != null) {
                    this._files.ScrollIntoView(fileToShow);
                    this._files.SelectedItem = fileToShow;
                }
            }
        }

        double _grpRatio = 0.3;

        private void _view_SizeChanged(object sender, SizeChangedEventArgs e) {
            var nsz = e.NewSize;
            if (e.WidthChanged) {
                if (nsz.Width>10) {
                    var trgW = _grpRatio * nsz.Width;
                    if (trgW > this._grpAreaCol.MaxWidth)
                        trgW = this._grpAreaCol.MaxWidth;
                    if (trgW < this._grpAreaCol.MinWidth)
                        trgW = this._grpAreaCol.MinWidth;
                    this._grpAreaCol.Width= new GridLength(trgW);
                }
            }
        }


        private void _view_LayoutUpdated(object sender, EventArgs e) {
            if (this._view.ActualWidth > 10) {
                _grpRatio = this._grpAreaCol.ActualWidth / this._view.ActualWidth;
                //System.Diagnostics.Debug.WriteLine(string.Format("grpRat: {0}", _grpRatio));
            }
        }

        double _picRatio = 0.415;

        private void _groupGrid_LayoutUpdated(object sender, EventArgs e) {
            if (this._groupGrid.ActualHeight > 10) {
                _picRatio = this._picAreaRow.ActualHeight / this._groupGrid.ActualHeight;
                //System.Diagnostics.Debug.WriteLine(string.Format("_picRatio: {0}", _picRatio));
            }
        }

        private void _groupGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
            var nsz = e.NewSize;
            if (e.HeightChanged) {
                if (nsz.Height > 10) {
                    var trgW = _picRatio * nsz.Height;
                    if (trgW > this._picAreaRow.MaxHeight)
                        trgW = this._picAreaRow.MaxHeight;
                    if (trgW < this._picAreaRow.MinHeight)
                        trgW = this._picAreaRow.MinHeight;
                    this._picAreaRow.Height = new GridLength(trgW);
                }
            }
        }

        private void _files_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            this.ViewModel?.RunDefaultActionOnCurrentMedia();
        }
    }
}

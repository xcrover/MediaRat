using Ops.NetCoe.LightFrame;
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

namespace XC.MediaRat.Views {
    /// <summary>
    /// Interaction logic for MediaSplitView.xaml
    /// </summary>
    public partial class MediaSplitView : UserControl, IBaseView {
        public MediaSplitView() {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the SizeChanged event of the _view control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
        private void _view_SizeChanged(object sender, SizeChangedEventArgs e) {
            this._sizeMonitor.Width = this._view.ActualWidth;
            this._sizeMonitor.Height = this.ActualHeight;
        }

        public MediaSplitVModel ViewModel {
            get { return this._view.DataContext as MediaSplitVModel; }
            set { this._view.DataContext = value; }
        }

        private void _media_Drop(object sender, DragEventArgs e) {
            MediaSplitVModel vm = this._view.DataContext as MediaSplitVModel;
            if (vm != null) {
                if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    vm.ProcessDroppedFiles(files);
                }
            }
        }

        public void SetViewModel(ViewModelBase viewModel) {
            this._view.DataContext = viewModel;
        }
    }
}

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
    /// Interaction logic for ImageProjectView.xaml
    /// </summary>
    public partial class ImageProjectView : UserControl, IBaseView {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageProjectView"/> class.
        /// </summary>
        public ImageProjectView() {
            InitializeComponent();
        }

        #region IBaseView Members

        /// <summary>
        /// Sets the view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public void SetViewModel(ViewModelBase viewModel) {
            this._view.DataContext= viewModel;
            ImageProjectVModel vm= viewModel as ImageProjectVModel;
            if (vm != null)
                vm.GetHighlightedItems = this.GetSelectedMedia;
        }

        #endregion

        /// <summary>
        /// Handles the SizeChanged event of the _view control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
        private void _view_SizeChanged(object sender, SizeChangedEventArgs e) {
            this._sizeMonitor.Width = this._view.ActualWidth;
            this._sizeMonitor.Height = this.ActualHeight;
        }

        private void _media_Drop(object sender, DragEventArgs e) {
            ImageProjectVModel vm = this._view.DataContext as ImageProjectVModel;
            if (vm != null) {
                if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    vm.ProcessDroppedFiles(files);
                }
            }
        }

        IList<MediaFile> GetSelectedMedia() {
            List<MediaFile> rz = new List<MediaFile>();
            MediaFile mf;
            foreach (var mit in this._media.SelectedItems) {
                mf = mit as MediaFile;
                if (mf != null)
                    rz.Add(mf);
            }
            return rz;
        }


        private void _media_BeginDragn(object sender, MouseButtonEventArgs e) {
            var selectedMediaFiles = GetSelectedMedia();
            if ((selectedMediaFiles == null) || (selectedMediaFiles.Count == 0)) return;
            System.Collections.Specialized.StringCollection pathes = new System.Collections.Specialized.StringCollection();
            foreach (var mf in selectedMediaFiles) {
                pathes.Add(mf.FullName);
            }
            DataObject dragObj = new DataObject();
            dragObj.SetFileDropList(pathes);
            DragDrop.DoDragDrop(this._media, dragObj, DragDropEffects.Copy);
        }

        private void _media_BeginDragn(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                var selectedMediaFiles = GetSelectedMedia();
                if ((selectedMediaFiles == null) || (selectedMediaFiles.Count == 0)) return;
                System.Collections.Specialized.StringCollection pathes = new System.Collections.Specialized.StringCollection();
                foreach (var mf in selectedMediaFiles) {
                    pathes.Add(mf.FullName);
                }
                DataObject dragObj = new DataObject();
                dragObj.SetFileDropList(pathes);
                DragDrop.DoDragDrop(this._media, dragObj, DragDropEffects.Copy);
            }
        }

        private void _media_BeginDrag(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                var selectedMediaFiles = GetSelectedMedia();
                if ((selectedMediaFiles == null) || (selectedMediaFiles.Count == 0)) return;
                System.Collections.Specialized.StringCollection pathes = new System.Collections.Specialized.StringCollection();
                foreach (var mf in selectedMediaFiles) {
                    pathes.Add(mf.FullName);
                }
                DataObject dragObj = new DataObject();
                dragObj.SetFileDropList(pathes);
                DragDrop.DoDragDrop(this._media, dragObj, DragDropEffects.Copy);
            }
        }

        void ExecuteCurrentMediaItem() {
            var mf = this._media.SelectedItem as MediaFile;
            if (mf != null) {
                ImageProjectVModel vm = this._view.DataContext as ImageProjectVModel;
                if (vm != null) {
                    vm.OpenMediaCmd.ExecuteIfCan(mf);
                }
            }
        }

        private void _media_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            ExecuteCurrentMediaItem();
        }

        private void _picPreview_MouseDown(object sender, MouseButtonEventArgs e) {
            if ((e.ChangedButton== MouseButton.Left)&&(e.ClickCount==2)) {
                ExecuteCurrentMediaItem();
            }
        }
    }
}

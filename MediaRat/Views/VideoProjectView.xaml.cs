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
    /// Interaction logic for VideoProjectView.xaml
    /// </summary>
    public partial class VideoProjectView : UserControl, IBaseView, VideoProjectVModel.IVProjectView {
        public VideoProjectView() {
            InitializeComponent();
        }

        private void _view_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            VideoProjectVModel vm = e.OldValue as VideoProjectVModel;
            if (vm != null) {
                vm.ViewUtil = null;
            }
            vm = e.NewValue as VideoProjectVModel;
            if (vm != null) {
                vm.ViewUtil = this;
            }
        }


        #region IBaseView Members

        /// <summary>
        /// Sets the view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public void SetViewModel(ViewModelBase viewModel) {
            this._view.DataContext = viewModel;
        }

        #endregion

        private void _sources_Drop(object sender, DragEventArgs e) {
            VideoProjectVModel vm = this._view.DataContext as VideoProjectVModel;
            if (vm != null) {
                if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    vm.AddSources(files);
                }
            }
        }

        #region IVProjectView Members

        /// <summary>
        /// Enumerates the selected content sources.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IContentSource> EnumerateSelectedSources() {
            IContentSource cs;
            foreach (var t in this._sources.SelectedItems) {
                cs = t as IContentSource;
                if (cs != null)
                    yield return cs;
            }
        }

        /// <summary>
        /// Enumerates the selected tracks.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<IMediaTrack> EnumerateSelectedTracks() {
            IMediaTrack tr;
            foreach (var t in this._tracks.SelectedItems) {
                tr = t as IMediaTrack;
                if (tr != null)
                    yield return tr;
            }
        }

        #endregion

        private void _tracks_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                var track = this._tracks.SelectedItem as IMediaTrack;
                if (track != null) {
                    string trackRef= string.Format("track:{0}", track.Id);
                    DragDrop.DoDragDrop(this._tracks, trackRef, DragDropEffects.Copy);
                }
            }
        }

    }
}

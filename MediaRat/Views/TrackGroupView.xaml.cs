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
    /// Interaction logic for TrackGroupView.xaml
    /// </summary>
    public partial class TrackGroupView : UserControl, IBaseView, TrackGroupVModel.ITrackGroupView {
        public TrackGroupView() {
            InitializeComponent();
        }

        private void _view_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            TrackGroupVModel vm = e.OldValue as TrackGroupVModel;
            if (vm != null)
                vm.ViewUtil = null;
            vm = e.NewValue as TrackGroupVModel;
            if (vm != null)
                vm.ViewUtil = this;
        }

        /// <summary>
        /// Gets the v model.
        /// </summary>
        /// <value>
        /// The v model.
        /// </value>
        public TrackGroupVModel VModel {
            get { return this._view.DataContext as TrackGroupVModel; }
        }

        #region IBaseView Members

        public void SetViewModel(ViewModelBase viewModel) {
            this._view.DataContext = viewModel;
        }

        #endregion

        #region ITrackGroupView Members

        /// <summary>
        /// Enumerates the selected tracks.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<IMediaTrack> EnumerateSelectedTracks() {
            IMediaTrack mt;
            foreach (var tr in this._tracks.SelectedItems) {
                mt = tr as IMediaTrack;
                if (mt != null)
                    yield return mt;
            }
        }

        #endregion

        private void _tracks_DragOver(object sender, DragEventArgs e) {
            DataGrid src = sender as DataGrid;
            if (src == null) return;
            var vm = this.VModel;
            if (vm == null) return;
            if (e.Data.GetDataPresent(typeof(string))) {

                e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
            }
        }

        private void _tracks_Drop(object sender, DragEventArgs e) {
            DataGrid src = sender as DataGrid;
            if (src == null) return;
            var vm = this.VModel;
            if (vm == null) return;
            if (e.Data.GetDataPresent(typeof(string))) {
                string trackMrk = e.Data.GetData(typeof(string)) as string;
                int trackId;
                if (trackMrk.StartsWith("track:")&&int.TryParse(trackMrk.Substring(6), out trackId)) {
                    var track= vm.Entity.Project.GetMediaTrack(trackId);
                    if (track!=null)
                        vm.TryAddTrack(track);
                }
            }
        }
    }
}

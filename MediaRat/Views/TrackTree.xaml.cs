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
    /// Interaction logic for TrackTree.xaml
    /// </summary>
    public partial class TrackTree : UserControl {
        public TrackTree() {
            InitializeComponent();
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            TrackListVModel vm;
            if (TryGetVModel(out vm)) {
                vm.EditCmd.ExecuteIfCan(this._tracks.SelectedItem as IMediaTrack);
            }
        }

        bool TryGetVModel(out TrackListVModel vm) {
            vm = this.DataContext as TrackListVModel;
            return vm != null;
        }

        private void _tracks_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            TrackListVModel vm;
            if (TryGetVModel(out vm)) {
                vm.CurrentTrack = e.NewValue as IMediaTrack;
            }
        }
    }
}

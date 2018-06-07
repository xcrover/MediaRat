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
using System.Windows.Shapes;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat.Views {
    /// <summary>
    /// Interaction logic for PopoutImage.xaml
    /// </summary>
    public partial class PopoutImage : Window, IManagedView {
        /// <summary>
        /// Initializes a new instance of the <see cref="PopoutImage"/> class.
        /// </summary>
        public PopoutImage() {
            InitializeComponent();
            this.ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip | System.Windows.ResizeMode.CanMinimize;
            var uhlp= AppContext.Current.GetServiceViaLocator<IUIHelper>();
            this.Owner = uhlp.GetMainWindow();
        }

        #region IBaseView Members

        /// <summary>
        /// Sets the view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void SetViewModel(ViewModelBase viewModel) {
            this.DataContext = viewModel;
        }

        #endregion

        #region IManagedView Members

        /// <summary>
        /// Ensures the visible.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void EnsureVisible() {
            this.BringIntoView();
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            ImageProjectVModel pvm = this._view.DataContext as ImageProjectVModel;
            if (pvm != null) {
                pvm.PopoutView = null;
            }
        }

        ///// <summary>
        ///// Manually closes a <see cref="T:System.Windows.Window" />.
        ///// </summary>
        //new public void Close() {
        //    base.Close();
        //}
        #endregion

        /// <summary>
        /// Handles the SizeChanged event of the _view control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
        private void _view_SizeChanged(object sender, SizeChangedEventArgs e) {
            this._sizeMonitor.Width = this._view.ActualWidth;
            this._sizeMonitor.Height = this.ActualHeight;
            //_scale.ScaleX = e.NewSize.Width / _picPreview.Source.Width;
            //_scale.ScaleY = e.NewSize.Height / _picPreview.Source.Height;
        }

        private void _picPreview_SourceUpdated(object sender, DataTransferEventArgs e) {
            //_scale.ScaleX = _view.Width / _picPreview.Source.Width;
            //_scale.ScaleY = _view.Height / _picPreview.Source.Height;
        }

        private void _scaleK_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            ImageProjectVModel pvm = this._view.DataContext as ImageProjectVModel;
            if (pvm != null)
                pvm.Scale.SetDefaultPos();
        }

        private void _picPreview_MouseDown(object sender, MouseButtonEventArgs e) {
            if ((e.ChangedButton == MouseButton.Left) && (e.ClickCount == 2)) {
                ImageProjectVModel pvm = this._view.DataContext as ImageProjectVModel;
                if (pvm != null)
                    pvm.OpenMediaCmd.ExecuteIfCan();
            }
        }
    }
}

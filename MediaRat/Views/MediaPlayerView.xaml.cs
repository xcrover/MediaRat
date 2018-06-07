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
    /// Interaction logic for MediaPlayerView.xaml
    /// http://stackoverflow.com/questions/39727054/c-sharp-mediaelement-why-does-play-sometimes-silently-fail-after-switching
    /// Or use DirectX: https://github.com/Sascha-L/WPF-MediaKit/tree/master/Source/MediaFoundation
    /// </summary>
    public partial class MediaPlayerView : UserControl, IBaseView {
        ///<summary>Timer</summary>
        private System.Threading.Timer _vTimer;
        bool _vTimerUpdate;

        ///<summary>Timer</summary>
        public System.Threading.Timer VTimer {
            get { return this._vTimer; }
            set { this._vTimer = value; }
        }
        
       
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPlayerView"/> class.
        /// </summary>
        public MediaPlayerView() {
            InitializeComponent();
        }

        void OnMediaOn() {
            if (this._vTimer == null) {
                System.Diagnostics.Debug.WriteLine("Media:VTimer Started");
                this._vTimer = new System.Threading.Timer((o) => this.Dispatcher.Invoke((Action)OnVTimer), null, 200, 200);
            }
        }

        void OnMediaOff() {
            if (this._vTimer != null) {
                System.Diagnostics.Debug.WriteLine("Media:VTimer Stopped");
                this._vTimer.Dispose();
                this._vTimer = null;
            }
        }

        void OnVTimer() {
            //System.Diagnostics.Debug.WriteLine("Media:OnVTimer");
            this._vTimerUpdate = true;
            this._mediaPosition.Value = this._player.Position.TotalMilliseconds;
            this._currTime.Content = this._player.Position.ToString(@"hh\:mm\:ss\.ff");
            this._vTimerUpdate = false;
        }

        /// <summary>
        /// Handles the MediaOpened event of the _player control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void _player_MediaOpened(object sender, RoutedEventArgs e) {
            try {
                this._mediaPosition.IsEnabled =
                this._volume.IsEnabled = this._player.IsLoaded;
                this._length.Content = this._player.NaturalDuration.ToString();
                //this._mediaPosition.Maximum = this._player.NaturalDuration.TimeSpan.TotalMilliseconds; // Throwing exceptions, needs check
                if (this._player.NaturalDuration.HasTimeSpan)
                    this._mediaPosition.Maximum = this._player.NaturalDuration.TimeSpan.TotalMilliseconds;
                else
                    this._mediaPosition.Maximum = 3600000; //1 hr
                //if (this._player.NaturalDuration.HasTimeSpan) { // No benefit over above. SHows only up to seconds
                //    this._length.Content = this._player.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss\.ff");
                //}
                OnMediaOn();
            }
            catch (Exception x) {
                if (this.ViewModel != null) {
                    this.ViewModel.Status.SetError(string.Format("Failed to open media. {0}: {1}", x.GetType().Name, x.Message), x);
                }
                EnsureReleaseMedia();
            }
        }

        /// <summary>
        /// Handles the MediaEnded event of the _player control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void _player_MediaEnded(object sender, RoutedEventArgs e) {
            OnMediaOff();
            this._player.Stop();
        }

        /// <summary>
        /// Handles the MediaFailed event of the _player control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExceptionRoutedEventArgs"/> instance containing the event data.</param>
        private void _player_MediaFailed(object sender, ExceptionRoutedEventArgs e) {
            if (this.ViewModel != null) {
                this.ViewModel.Status.SetError(string.Format("Failed to open media. {0}: {1}", e.ErrorException.GetType().Name, e.ErrorException.Message), e.ErrorException);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the _pbPlay control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void _pbPlay_Click(object sender, RoutedEventArgs e) {
            this._player.Play();
            this._player.Volume = (double)this._volume.Value;
            OnMediaOn();
        }

        /// <summary>
        /// Handles the Click event of the _pbPause control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void _pbPause_Click(object sender, RoutedEventArgs e) {
            this._player.Pause();
        }

        /// <summary>
        /// Handles the Click event of the _pbStop control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void _pbStop_Click(object sender, RoutedEventArgs e) {
            OnMediaOff();
            this._player.Stop();
        }

        private void _player_Unloaded(object sender, RoutedEventArgs e) {
            OnMediaOff();
            this._player.Stop();
            this._player.Source = null;
            VsTrace("Player source released: _player_Unloaded");
        }


        ///// <summary>
        ///// Handles the ValueChanged event of the _volume control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="RoutedPropertyChangedEventArgs{System.Double}"/> instance containing the event data.</param>
        //private void _volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
        //    this._player.Volume = (double)this._volume.Value;
        //}

        /// <summary>
        /// Handles the ValueChanged event of the _mediaPosition control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedPropertyChangedEventArgs{System.Double}"/> instance containing the event data.</param>
        private void _mediaPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (!this._vTimerUpdate) {
                TimeSpan ts;
                this._player.Position = ts= TimeSpan.FromMilliseconds(this._mediaPosition.Value);
                this._currTime.Content = ts.ToString();
            }
        }

        #region IBaseView Members

        /// <summary>
        /// Sets the view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void SetViewModel(ViewModelBase viewModel) {
            this.ViewModel = viewModel as ImageProjectVModel;
        }

        #endregion


        private void Control_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ImageProjectVModel vm;
            vm = e.OldValue as ImageProjectVModel;
            if (vm != null) {
                vm.PropertyChanged -= vm_PropertyChanged;
            }
            vm = e.NewValue as ImageProjectVModel;
            if (vm != null) {
                vm.PropertyChanged += vm_PropertyChanged;
            }
            UpdateView();
        }

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        ImageProjectVModel ViewModel {
            get { return this.DataContext as ImageProjectVModel; }
            set { this.DataContext = value; }
        }

        void EnsureReleaseMedia() {
            if (this._player.Source!=null) {
                this._player.Stop();
                this._player.Close();
                this._player.Source = null;
                VsTrace("Player source released: EnsureReleaseMedia");
            }
        }

        void UpdateView() {
            ImageProjectVModel vm= this.ViewModel;
            VsTrace("UpdateView");
            if (vm!=null) {
                if ((vm.CurrentMedia == null) || (vm.CurrentMedia.MediaType == MediaTypes.Image)) {
                    OnMediaOff();
                    EnsureReleaseMedia();
                    this.Visibility = Visibility.Collapsed;
                }
                else {
                    EnsureReleaseMedia();
                    this._player.Source = null;
                    this._player.Source = new Uri(vm.CurrentMedia.FullName, UriKind.Absolute);
                    this._mediaPosition.Value = 0;
                    this._player.Play();
                    OnMediaOn();
                    this._player.Volume = this._volume.Value;
                    this.Visibility = Visibility.Visible;
                }
            }
        }

        void vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "CurrentMedia") {
                this.UpdateView();
            }
        }

        void VsTrace(string fmt, params object[] args) {
            System.Diagnostics.Debug.WriteLine(string.Format(fmt, args));
        }

     }
}

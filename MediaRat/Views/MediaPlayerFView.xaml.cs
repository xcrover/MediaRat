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
    /// Interaction logic for MediaPlayerFView.xaml
    /// </summary>
    public partial class MediaPlayerFView : UserControl {
        ///<summary>Timer</summary>
        private System.Threading.Timer _vTimer;
        bool _vTimerUpdate;
        ///<summary>Status</summary>
        private IMessagePresenter _status;
        private StepScale _speedScale;
        ///<summary>Status</summary>
        public IMessagePresenter Status {
            get { return this._status; }
            set { this._status = value; }
        }


        ///<summary>Timer</summary>
        public System.Threading.Timer VTimer {
            get { return this._vTimer; }
            set { this._vTimer = value; }
        }

        public MediaPlayerFView() {
            InitializeComponent();
            _speedScale = this._view.TryFindResource("SpeedScale") as StepScale;
            _speedScale.Init(15, 1.0, 1.25);
            _speedScale.PropertyChanged += Sv_PropertyChanged;
        }

        private void Sv_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Value") {
                StepScale sv = ((StepScale)sender)?? _speedScale;
                this._player.SpeedRatio = sv.Value;
            }
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
            this._currTime.Text = ToTCStr(this._player.Position); //.ToHumanString(); // .ToString(@"hh\:mm\:ss\.ff");
            this._vTimerUpdate = false;
        }

        void SetError(string fmt, params object[] args) {
            if (this.Status != null) {
                this.Status.SetError(string.Format(fmt, args), null);
            }
        }

        void SetError(string operation, Exception x) {
            if (this.Status != null) {
                this.Status.SetError(x.ToShortMsg(operation), x);
            }
        }

        string ToTCStr(TimeSpan src) {
            if (src.Hours>0)
                return src.ToString(@"hh\:mm\:ss\.ff");
            else
                return src.ToString(@"mm\:ss\.ff");
        }

        /// <summary>
        /// Handles the MediaOpened event of the _player control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void _player_MediaOpened(object sender, RoutedEventArgs e) {
            try {
                this._mediaPosition.IsEnabled = this._volume.IsEnabled = this._player.IsLoaded;
                this._length.Text = this._player.NaturalDuration.HasTimeSpan ? ToTCStr(this._player.NaturalDuration.TimeSpan) : this._player.NaturalDuration.ToString();
                //this._mediaPosition.Maximum = this._player.NaturalDuration.TimeSpan.TotalMilliseconds; // Throwing exceptions, needs check
                this._mediaPosition.Maximum = GetDurationS() * 1000;
                OnMediaOn();
            }
            catch (Exception x) {
                SetError("open media", x);
            }
        }

        double GetDurationS() {
            if (this._player.NaturalDuration.HasTimeSpan)
                return this._player.NaturalDuration.TimeSpan.TotalSeconds;
            else
                return 3600; //1 hr
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
            SetError("open media", e.ErrorException);
            e.Handled = true;
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
                this._player.Position = ts = TimeSpan.FromMilliseconds(this._mediaPosition.Value);
                this._currTime.Text = ts.ToHumanString(); // .ToString();
            }
        }

        #region IBaseView Members

        /// <summary>
        /// Sets the view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public void SetViewModel(ViewModelBase viewModel) {
            this.ViewModel = viewModel as ISourceProvider;
        }

        #endregion


        private void Control_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ISourceProvider vm;
            vm = e.OldValue as ISourceProvider;
            if (vm != null) {
                vm.PropertyChanged -= vm_PropertyChanged;
            }
            vm = e.NewValue as ISourceProvider;
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
        ISourceProvider ViewModel {
            get { return this.DataContext as ISourceProvider; }
            set { this.DataContext = value; }
        }

        void UpdateView() {
            ISourceProvider vm = this.ViewModel;
            if (vm != null) {
                ISourceRef srf = vm.ActiveSource;
                if ((srf == null) || (srf.MediaType == MediaTypes.Image)) {
                    OnMediaOff();
                    this._player.Stop();
                    this._player.Source = null;
                    this.Visibility = Visibility.Collapsed;
                }
                else {
                    this._player.Source = null;
                    this._player.Source = new Uri(srf.SourcePath, UriKind.Absolute);
                    this._mediaPosition.Value = 0;
                    this._player.Play();
                    OnMediaOn();
                    this._player.Volume = this._volume.Value;
                    this.Visibility = Visibility.Visible;
                }
            }
        }

        void vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "ActiveSource") {
                this.UpdateView();
            }
        }

        double GetStepMultiplier() {
            bool isShift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            double rz = 0.1;
            if (isShift) {
                rz *= (isCtrl) ? 200 : 10;
            }
            else if (isCtrl)
                rz *= 100;
            return rz;
        }

        void SetStepPosition(double ts) {
            this._player.Position = TimeSpan.FromSeconds(ts);
            this._player.Play();
            this._player.Pause();
        }

        private void _pbStepLeft_Click(object sender, RoutedEventArgs e) {
            double ts = this._player.Position.TotalSeconds;
            double step = _speedScale.Value* GetStepMultiplier();
            ts -= step;
            if (ts < 0)
                ts= 0;
            SetStepPosition(ts);
        }

        private void _pbStepRight_Click(object sender, RoutedEventArgs e) {
            double ts = this._player.Position.TotalSeconds;
            double mx = this.GetDurationS();
            double step = _speedScale.Value* GetStepMultiplier();
            ts += step;
            if (ts > mx)
                ts = mx;
            SetStepPosition(ts);
        }
    }
}

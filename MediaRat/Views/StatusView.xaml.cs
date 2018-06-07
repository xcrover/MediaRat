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
    /// Interaction logic for StatusView.xaml
    /// </summary>
    public partial class StatusView : UserControl {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusView"/> class.
        /// </summary>
        public StatusView() {
            InitializeComponent();
        }

        ///<summary>Model</summary>
        public StatusVModel Model {
            get {
                return this._statusMessage.DataContext as StatusVModel;
            }
            set {
                this.DataContext = value;
            }
        }


        /// <summary>
        /// Handles the PropertyChanged event of the Model control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Status") {
                UpdateStatus();
            }
        }



        protected virtual void UpdateStatus() {
            UserMessageTypes status = (this.Model == null) ? UserMessageTypes.Undefined : this.Model.Status;
            if (status == UserMessageTypes.Undefined) {
                this.Visibility = Visibility.Collapsed;
                return;
            }
            this.Visibility = Visibility.Visible;
            string key = "StatusMessage" + status.ToString();
            this._statusMessage.Style = (Style)App.Current.Resources[key];
        }


        private void _statusMessage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            DetachVModel(e.OldValue as StatusVModel);
            AttachVModel(e.NewValue as StatusVModel);
        }

        void AttachVModel(StatusVModel vm) {
            if (vm != null) {
                vm.PropertyChanged += Model_PropertyChanged;
                UpdateStatus();
            }
        }

        void DetachVModel(StatusVModel vm) {
            if (vm != null) {
                vm.PropertyChanged -= Model_PropertyChanged;
            }
        }

        private void _statusMessage_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var vm = this.Model;
            if (vm != null)
                vm.Clear();
        }
    }
}

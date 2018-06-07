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
    /// Interaction logic for PopupView.xaml
    /// </summary>
    public partial class PopupView : Window, IManagedView {
        /// <summary>
        /// Initializes a new instance of the <see cref="PopupView"/> class.
        /// </summary>
        public PopupView() {
            InitializeComponent();
            this.Loaded += PopupView_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the PopupView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void PopupView_Loaded(object sender, RoutedEventArgs e) {
            UserControl uctrl = this._content.Content as UserControl;
            if (uctrl != null) {
                double mw = uctrl.MinWidth * 1.1, mh = uctrl.MinHeight * 1.1;
                if (this.Width < mw) this.Width = mw;
                if (this.Height < mh) this.Height = mh;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PopupView"/> class.
        /// </summary>
        /// <param name="vmodel">The vmodel.</param>
        public PopupView(WorkspaceViewModel vmodel)
            : this() {
                this.SetViewModel(vmodel);
        }

        /// <summary>
        /// Handles the RequestClose event of the vmodel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void vmodel_RequestClose(object sender, EventArgs e) {
            this.Close();
        }


        #region IManagedView Members

        public void EnsureVisible() {
            this.BringIntoView();
        }

        #endregion

        #region IBaseView Members

        public void SetViewModel(ViewModelBase viewModel) {
            var factory = AppContext.Current.GetServiceViaLocator<ViewFactory>();
            this._content.Content = factory.CreateView(viewModel);
            //this.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            WorkspaceViewModel vmodel = viewModel as WorkspaceViewModel;
            if (vmodel != null) {
                vmodel.RequestClose += new EventHandler(vmodel_RequestClose);
                this.Title = vmodel.Title;
            }
        }

        #endregion
    }
}

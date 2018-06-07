using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Ops.NetCoe.LightFrame;
using XC.MediaRat;
using XC.MediaRat.Views;
using XC.MediaRat.ViewModels;

namespace XC.MediaRat {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IUIBusSubscriber, IUIHelper {
        /// <summary>Handles the Startup event of the Application control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StartupEventArgs"/> instance containing the event data.</param>
        private void Application_Startup(object sender, StartupEventArgs e) {
            CultureInfo culutreInfo = System.Threading.Thread.CurrentThread.CurrentCulture.Clone() as CultureInfo;
            culutreInfo.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            culutreInfo.DateTimeFormat.ShortTimePattern = "HH:mm:ss";
            System.Threading.Thread.CurrentThread.CurrentCulture = culutreInfo;

            var ctx = Bootstrap.EnsureContext();
            var bus = ctx.GetServiceViaLocator<UIBus>();
            bus.AddSubscriber(this);
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            var mvm = AppContext.Current.GetServiceViaLocator<MainVModel>();
            IMessagePresenter statusUi = mvm.Status;
            if ((mvm.ActiveWorkspace!=null)&&(mvm.ActiveWorkspace.Status!=null)) {
                statusUi = mvm.ActiveWorkspace.Status;
            }
            statusUi.SetError(e.Exception.ToShortMsg(), e.Exception);
            e.Handled = true;
        }

        /// <summary>
        /// Called on UI bus message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void OnMessage(ApplicationMessage<UIBusMessageTypes> message) {
            try {
                switch (message.MessageType) {
                    case UIBusMessageTypes.InformationRequest: {
                            InformationRequest irqt = message.Payload as InformationRequest;
                            if (irqt != null) {
                                WorkspaceViewModel vm = irqt.Tag as WorkspaceViewModel;
                                if (vm != null) {
                                    PopupView pw = new PopupView(vm);
                                    pw.Owner = this.MainWindow;
                                    pw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                                    if (pw.ShowDialog() == true) {
                                        irqt.CompleteMethod(irqt);
                                    }
                                }
                                else if (irqt.ResultType == typeof(System.IO.File)) {
                                    SelectFileName(irqt);
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception x) {
                AppContext.Current.LogTechError(string.Format("Technical error {0}: {1}", x.GetType().Name, x.Message), x);
                System.Diagnostics.Debug.WriteLine(x.GetDetails());
            }
        }


        void SelectFileName(InformationRequest request) {
            FileAccessRequest far = (FileAccessRequest)request.Tag;
            if (far.IsForReading) {
                OpenFileDialog ofd = new OpenFileDialog() {
                    Filter = far.ExtensionFilter,
                    Multiselect = far.IsMultiSelect
                };
                if (far.ExtensionFilterIndex > 0)
                    ofd.FilterIndex = far.ExtensionFilterIndex;
                if (ofd.ShowDialog() == true) {
                    request.Result = ofd.FileName;
                    request.CompleteMethod(request);
                }
            }
            else {
                SaveFileDialog sfd = new SaveFileDialog() {
                    DefaultExt = string.IsNullOrEmpty(far.SuggestedFileName) ? string.Empty : System.IO.Path.GetExtension(far.SuggestedFileName),
                    Filter = far.ExtensionFilter
                };
                if (far.ExtensionFilterIndex > 0)
                    sfd.FilterIndex = far.ExtensionFilterIndex;
                if (sfd.ShowDialog() == true) {
                    //Func<Stream> openWriteStream= () => sfd.OpenFile();
                    request.Result = sfd.FileName;
                    request.CompleteMethod(request);
                }
            }
        }



        #region IUIHelper Members

        /// <summary>
        /// Gets the main window.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public MainWindow GetMainWindow() {
            MainWindow rz;
            foreach (var w in this.Windows) {
                rz= w as MainWindow;
                if (rz!=null) return rz;
            }
            return null;
        }

        /// <summary>
        /// Selects the folder.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="initialFolder">The initial folder.</param>
        /// <returns></returns>
        public bool SelectFolder(string description, ref string initialFolder) {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog() {
                ShowNewFolderButton = true,
                Description = description
            };
            if (!string.IsNullOrEmpty(initialFolder)) {
                fbd.SelectedPath = initialFolder;
            }
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                initialFolder = fbd.SelectedPath;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the user confirmation.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <returns></returns>
        public bool GetUserConfirmation(string question) {
            return MessageBox.Show(GetMainWindow(), question, AppContext.Current.ApplicationTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK;
        }

        public bool TryAskText(string title, string label, string initVal, Action<string> newVal) {
            TextRqtVModel vm = new TextRqtVModel(title, label, initVal, newVal);
            PopupView pw = new PopupView(vm);
            var rz = pw.ShowDialog();
            return rz==true;
        }

        #endregion
    }
}

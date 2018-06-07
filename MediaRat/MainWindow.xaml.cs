using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

namespace XC.MediaRat {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            this.SetViewModel(Bootstrap.EnsureContext().GetServiceViaLocator<MainVModel>());
        }

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        /// <value>The view model.</value>
        public MainVModel ViewModel {
            get { return (MainVModel)this._view.DataContext; }
            set {
                MainVModel current = this.ViewModel;
                if (current != null) {
                    current.Workspaces.CollectionChanged -= Workspaces_CollectionChanged;
                    //current.PropertyChanged -= viewModel_PropertyChanged;
                }
                this._workspaces.Items.Clear();
                this.DataContext= this._view.DataContext = value;
                if (value != null) {
                    value.Workspaces.CollectionChanged += Workspaces_CollectionChanged;
                    value.RequestClose += vmodel_RequestClose;
                    //value.PropertyChanged += viewModel_PropertyChanged;
                    AddWorkspaces(value.Workspaces);
                }
            }
        }

        void vmodel_RequestClose(object sender, EventArgs e) {
            this.Close();
        }

        void AddWorkspaces(IEnumerable<WorkspaceViewModel> workspaces) {
            TabItem ti;
            foreach (WorkspaceViewModel workspace in workspaces) {
                ti = CreateTabItem(workspace);
                try {
                    //Log("Adding {0} to tab.Items", ti);
                    this._workspaces.Items.Add(ti);
                    //Log("Added {0} to tab.Items", ti);
                    //this._workspaces.SelectedItem = ti;
                }
                catch (Exception x) {
                    AppContext.Current.LogTechError("Failed to add workspace", x);
                }
            }
        }

        void Workspaces_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            TabItem ti;
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null && e.NewItems.Count != 0)
                        AddWorkspaces(e.NewItems.EnumerateAs<WorkspaceViewModel>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null && e.OldItems.Count != 0) {
                        foreach (WorkspaceViewModel workspace in e.OldItems) {
                            ti = FindExistingTabItem(workspace);
                            if (ti != null) {
                                //Log("Removing {0} from tab.Items", ti);
                                this._workspaces.Items.Remove(ti);
                                //Log("Removed {0} from tab.Items", ti);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems != null && e.NewItems.Count != 0)
                        foreach (WorkspaceViewModel workspace in e.NewItems) {
                            this._workspaces.Items.Add(CreateTabItem(workspace));
                        }
                    if (e.OldItems != null && e.OldItems.Count != 0) {
                        foreach (WorkspaceViewModel workspace in e.OldItems) {
                            ti = FindExistingTabItem(workspace);
                            if (ti != null) {
                                //Log("Removing {0} from tab.Items", ti);
                                this._workspaces.Items.Remove(ti);
                                //Log("Removed {0} from tab.Items", ti);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }

            this._workspaces.Visibility = (this._workspaces.Items.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
        }


        TabItem FindExistingTabItem(WorkspaceViewModel workspace) {
            foreach (TabItem ti in this._workspaces.Items) {
                if (ti.Header == workspace)
                    return ti;
            }
            return null;
        }

        TabItem CreateTabItem(WorkspaceViewModel workspace) {
            ViewFactory factory = AppContext.Current.GetServiceViaLocator<ViewFactory>();
            IBaseView view = factory.CreateView(workspace);
            TabItem ti = new TabItem();
            ti.Content = view;
            ti.Header = workspace;
            ti.DataContext = workspace;
            // apply the template to make the tab closable
            //ti.HeaderTemplate = (DataTemplate)App.Current.Resources["ClosableTabItemTemplate"];

            //// apply the styling from COEResourceDictionary
            //ti.Template = (ControlTemplate)App.Current.Resources["TabItemWorkspaceTemplate"];
            return ti;
        }

        /// <summary>
        /// Sets the view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public void SetViewModel(ViewModelBase viewModel) {
            ViewModel = (MainVModel)viewModel;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            var vm= this.ViewModel;
            e.Cancel= !vm.GetCloseConfirmation();
        }

    }
}

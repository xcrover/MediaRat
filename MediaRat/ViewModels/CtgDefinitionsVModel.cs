using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
 
    ///<summary>View Model for Category Definitions</summary>
    public class CtgDefinitionsVModel : WorkspaceViewModel {
        ///<summary>Media Project</summary>
        private MediaProject _project;
        ///<summary>Current Category Definition</summary>
        private CategoryDefinition _currentCategoryDefinition;
        ///<summary>Category Items</summary>
        private ObservableCollection<CategoryEntry> _categoryItems;
        ///<summary>Current Category Item</summary>
        private CategoryEntry _currentCategoryItem;

        ///<summary>Current Category Item</summary>
        public CategoryEntry CurrentCategoryItem {
            get { return this._currentCategoryItem; }
            set {
                if (this._currentCategoryItem != value) {
                    this._currentCategoryItem = value;
                    this.FirePropertyChanged("CurrentCategoryItem");
                    this.ResetViewState();
                }
            }
        }
        

        ///<summary>Category Items</summary>
        public ObservableCollection<CategoryEntry> CategoryItems {
            get { return this._categoryItems; }
            set {
                if (this._categoryItems != value) {
                    this._categoryItems = value;
                    this.FirePropertyChanged("CategoryItems");
                }
            }
        }
        

        ///<summary>Current Category Definition</summary>
        public CategoryDefinition CurrentCategoryDefinition {
            get { return this._currentCategoryDefinition; }
            set {
                if (this._currentCategoryDefinition != value) {
                    this.UpdateEntity();
                    this._currentCategoryDefinition = value;
                    this.UpdateView();
                    this.FirePropertyChanged("CurrentCategoryDefinition");
                    this.ResetViewState();
                }
            }
        }
        

        ///<summary>Media Project</summary>
        public MediaProject Project {
            get { return this._project; }
            set {
                if (this._project != value) {
                    this._project = value;
                    this.ResetProject();
                    this.FirePropertyChanged("Project");
                }
            }
        }
        
        #region Commands
        ///<summary>Command VModels</summary>
        private ObservableCollection<CommandVModel> _commandVModels;
        ///<summary>Exit Command</summary>
        private RelayCommand _exitCmd;
        ///<summary>Add category Command</summary>
        private RelayCommand _addCategoryCmd;
        ///<summary>Delete Category Command</summary>
        private RelayCommand _deleteCategoryCmd;
        ///<summary>Add category item Command</summary>
        private RelayCommand _addCategoryItemCmd;
        ///<summary>Delete Category Item Command</summary>
        private RelayCommand _deleteCategoryItemCmd;


        ///<summary>Command VModels</summary>
        public ObservableCollection<CommandVModel> CommandVModels {
            get { return this._commandVModels; }
            set {
                if (this._commandVModels != value) {
                    this._commandVModels = value;
                    this.FirePropertyChanged("CommandVModels");
                }
            }
        }

        ///<summary>Exit Command</summary>
        public RelayCommand ExitCommand {
            get { return this._exitCmd; }
        }

        ///<summary>Add category Command</summary>
        public RelayCommand AddCategoryCmd {
            get { return this._addCategoryCmd; }
        }

        ///<summary>Delete Category Command</summary>
        public RelayCommand DeleteCategoryCmd {
            get { return this._deleteCategoryCmd; }
        }


        ///<summary>Add category item Command</summary>
        public RelayCommand AddCategoryItemCmd {
            get { return this._addCategoryItemCmd; }
        }

        ///<summary>Delete Category Item Command</summary>
        public RelayCommand DeleteCategoryItemCmd {
            get { return this._deleteCategoryItemCmd; }
        }


        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="CtgDefinitionsVModel"/> class.
        /// </summary>
        public CtgDefinitionsVModel()
            : this("Category Definitions") {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CtgDefinitionsVModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public CtgDefinitionsVModel(string title) {
            this.Title = title;
            Init();
            this.InitCommands();
        }

        /// <summary>
        /// Initialize business content.
        /// This method can be called more than once
        /// </summary>
        void Init() {
        }
        #endregion

        #region Command plumbing
        void DoExit(object prm) {
            this.OnRequestClose();
        }

        ///<summary>Execute Add category Command</summary>
        void DoAddCategoryCmd(object prm = null) {
            var ctg = new CategoryDefinition() {
                Marker = "ctg1",
                Title = "MyCategory",
                Values = new ObservableCollection<string>()
            };
            this.Project.CategoryDefinitions.Add(ctg);
            this.CurrentCategoryDefinition = ctg;
        }

        ///<summary>Check if Add category Command can be executed</summary>
        bool CanAddCategoryCmd(object prm = null) {
            return true;
        }
 

        ///<summary>Execute Delete Category Command</summary>
        void DoDeleteCategoryCmd(object prm = null) {
        }

        ///<summary>Check if Delete Category Command can be executed</summary>
        bool CanDeleteCategoryCmd(object prm = null) {
            return this.CurrentCategoryDefinition!=null;
        }

        ///<summary>Execute Add category item Command</summary>
        void DoAddCategoryItemCmd(object prm = null) {
            this.CategoryItems.Add(new CategoryEntry());
        }

        ///<summary>Check if Add category item Command can be executed</summary>
        bool CanAddCategoryItemCmd(object prm = null) {
            return (this.CurrentCategoryDefinition != null) && (this.CategoryItems!=null);
        }

        ///<summary>Execute Delete Category Item Command</summary>
        void DoDeleteCategoryItemCmd(object prm = null) {
            this.CategoryItems.Remove(this.CurrentCategoryItem);
        }

        ///<summary>Check if Delete Category Item Command can be executed</summary>
        bool CanDeleteCategoryItemCmd(object prm = null) {
            return this.CurrentCategoryItem!=null;
        }

        /// <summary>
        /// Enumerate all the available commands
        /// </summary>
        IEnumerable<RelayCommand> EnumerateCommands() {
            yield return this.AddCategoryCmd;
            yield return this.DeleteCategoryCmd;
            yield return this.AddCategoryItemCmd;
            yield return this.DeleteCategoryItemCmd;
            yield return this.ExitCommand;
        }

        /// <summary>
        /// Initialize commands infrastructure
        /// </summary>
        void InitCommands() {
            this._addCategoryCmd = new RelayCommand(UIOperations.AddCategory, DoAddCategoryCmd, CanAddCategoryCmd);
            this._deleteCategoryCmd = new RelayCommand(UIOperations.DeleteCategory, DoDeleteCategoryCmd, CanDeleteCategoryCmd);
            this._addCategoryItemCmd = new RelayCommand(UIOperations.AddItem, DoAddCategoryItemCmd, CanAddCategoryItemCmd);
            this._deleteCategoryItemCmd = new RelayCommand(UIOperations.DeleteItem, DoDeleteCategoryItemCmd, CanDeleteCategoryItemCmd);
            this._exitCmd = new RelayCommand(UIOperations.Exit, DoExit, (p) => true);

            ObservableCollection<CommandVModel> cmdVms = new ObservableCollection<CommandVModel>();
            cmdVms.Add(new CommandVModel(ExitCommand));
            //cmdVms.Add(new CommandVModel(ClonetCmd) { Name = "Clone", Description = "Clone workspace" });
            CommandVModels = cmdVms;
        }

        /// <summary>
        /// Reset presentation attributes according to the current state
        /// </summary>
        void ResetViewState() {
            foreach (var cmd in this.EnumerateCommands()) cmd.Reset();
        }

        /// <summary>Called when <see cref="IsBusy"/> changed.</summary>
        /// <param name="newValue">New value of <see cref="IsBusy"/>.</param>
        protected override void OnIsBusyChanged(bool newValue) {
            base.OnIsBusyChanged(newValue);
            ResetViewState();
        }

        /// <summary>
        /// Find Command
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public override ICommand FindCommand(Func<ICommand, bool> filter) {
            return this.EnumerateCommands().FirstOrDefault(filter);
        }

        #endregion

        #region Operations

        /// <summary>
        /// Resets the project.
        /// </summary>
         public void ResetProject() {

        }

        /// <summary>
        /// Updates the view.
        /// </summary>
        public void UpdateView() {
            ObservableCollection<CategoryEntry> cel = new ObservableCollection<CategoryEntry>();
            if ((this.CurrentCategoryDefinition != null) && (this.CurrentCategoryDefinition.Values!=null)) {
                HashSet<string> existing = new HashSet<string>();
                foreach (var cd in from d in this.CurrentCategoryDefinition.Values orderby d select d) {
                    if (existing.Add((cd ?? string.Empty).ToLower()))
                        cel.Add(new CategoryEntry() { Name = cd });
                }
            }
            this.CategoryItems = cel;
        }

        /// <summary>
        /// Updates the entity.
        /// </summary>
        public void UpdateEntity() {
            if ((this.CurrentCategoryDefinition != null) && (this.CategoryItems != null)) {
                ObservableCollection<string> items= new ObservableCollection<string>();
                HashSet<string> existing = new HashSet<string>();
                foreach (var ci in from d in this.CategoryItems orderby d.Name select d.Name) {
                    if (existing.Add((ci ?? string.Empty).ToLower()))
                        items.Add(ci);
                }
                this.CurrentCategoryDefinition.Values = (items.Count>0) ? items : null;
            }
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Category Item visual helper
        /// </summary>
        public class CategoryEntry : NotifyPropertyChangedBase {
            ///<summary>Name</summary>
            private string _name;

            ///<summary>Name</summary>
            public string Name {
                get { return this._name; }
                set {
                    if (this._name != value) {
                        this._name = value;
                        this.FirePropertyChanged("Name");
                    }
                }
            }
            

        }

        #endregion
    }

    
}

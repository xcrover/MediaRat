using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {

    ///<summary>View Model for Media File information panel (ratings, categories etc.)</summary>
    public class MediaInfoVModel : WorkspaceViewModel {
        ///<summary>Media Project</summary>
        private MediaProject _project;
        ///<summary>Media File</summary>
        private MediaFile _entity;
        ///<summary>Entity ratings</summary>
        private ObservableCollection<RatingEntry> _ratings;
        ///<summary>Category Pickers</summary>
        private ObservableCollection<CtgPicker> _categories;


        #region Properties
        ///<summary>Media File</summary>
        public MediaFile Entity {
            get { return this._entity; }
            set {
                if (this._entity != value) {
                    this.UpdateEntity();
                    this._entity = value;
                    this.FirePropertyChanged("Entity");
                    this.UpdateView();
                }
            }
        }
        

        ///<summary>Media Project</summary>
        public MediaProject Project {
            get { return this._project; }
            set {
                if (this._project != value) {
                    this._project = value;
                    this.FirePropertyChanged("Project");
                    this.ResetDefinitions();
                }
            }
        }


        ///<summary>Entity ratings</summary>
        public ObservableCollection<RatingEntry> Ratings {
            get { return this._ratings; }
            set {
                if (this._ratings != value) {
                    this._ratings = value;
                    this.FirePropertyChanged("Ratings");
                }
            }
        }

        ///<summary>Category Pickers</summary>
        public ObservableCollection<CtgPicker> Categories {
            get { return this._categories; }
            set {
                if (this._categories != value) {
                    this._categories = value;
                    this.FirePropertyChanged("Categories");
                }
            }
        }
        

        #endregion

        #region Commands
        ///<summary>Command VModels</summary>
        private ObservableCollection<CommandVModel> _commandVModels;
        ///<summary>Exit Command</summary>
        private RelayCommand _exitCmd;

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

        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaInfoVModel"/> class.
        /// </summary>
        public MediaInfoVModel()
            : this("MediaInfoVModel") {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaInfoVModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public MediaInfoVModel(string title) {
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

        /// <summary>
        /// Enumerate all the available commands
        /// </summary>
        IEnumerable<RelayCommand> EnumerateCommands() {
            yield return this.ExitCommand;
        }

        /// <summary>
        /// Initialize commands infrastructure
        /// </summary>
        void InitCommands() {
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
            foreach (var cmd in this.EnumerateCommands()) cmd.Reset(null);
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
        /// Resets the definitions by the Media Project.
        /// </summary>
        public void ResetDefinitions() {
            ObservableCollection<RatingEntry> ratings = new ObservableCollection<RatingEntry>();
            ObservableCollection<CtgPicker> categories = new ObservableCollection<CtgPicker>();
            if (this.Project != null) {
                if (this.Project.RatingDefinitions != null) {
                    foreach (var q in from rd in this.Project.RatingDefinitions orderby rd.Title select rd) {
                        ratings.Add(new RatingEntry() {
                            Definition = q
                        });
                    }
                }

                if (this.Project.CategoryDefinitions != null) {
                    foreach (var q in from cd in this.Project.CategoryDefinitions orderby cd.Title select cd) {
                        categories.Add(new CtgPicker(q));
                    }

                }
            }
            this.Ratings = ratings;
            this.Categories = categories;
            this.UpdateView();
        }

        /// <summary>
        /// Updates the view.
        /// </summary>
        public void UpdateView() {
            RatingEntry tre;
            foreach (var rd in this.Ratings) {
                rd.Mark = null;
            }
            if (this.Entity != null) { 
                if (this.Ratings!=null) {
                    foreach (var rv in this.Entity.Ratings) {
                        tre = this.Ratings.FirstOrDefault((r) => r.Definition == rv.Key);
                        if (tre != null) {
                            tre.Mark = rv.Value;
                        }
                    }
                }

                if (this.Categories != null) {
                    foreach (var ctg in this.Categories) ctg.ClearSelection();
                    if (this.Entity.Categories != null) {
                        foreach (var ectg in this.Entity.Categories) {
                            var vCtg = this.Categories.FirstOrDefault((t) => t.Definition == ectg.Key);
                            if (vCtg != null)
                                vCtg.SetSelected(ectg.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the entity.
        /// </summary>
        public void UpdateEntity() {
            if (this.Entity != null) {
                if (this.Ratings != null) {
                    foreach (var re in this.Ratings) {
                        var rv = this.Entity.Ratings.FirstOrDefault((r) => r.Key == re.Definition);
                        if (rv == null) {
                            if (!re.Mark.HasValue) continue;
                            rv = new KeyValuePairX<RatingDefinition, int>() { Key = re.Definition, Value = re.Mark.Value };
                            this.Entity.Ratings.Add(rv);
                        }
                        else {
                            if (re.Mark.HasValue) {
                                rv.Value = re.Mark.Value;
                            }
                            else {
                                this.Entity.Ratings.Remove(rv);
                            }
                        }
                    }
                }

                if (this.Categories != null) {
                    foreach (var vCtg in this.Categories) {
                        HashSet<string> slc = new HashSet<string>(vCtg.EnumerateSelected());
                        var ectg = this.Entity.Categories.FirstOrDefault((t) => t.Key == vCtg.Definition);
                        if (ectg == null) {
                            ectg = new KeyValuePairX<CategoryDefinition, HashSet<string>>() { Key = vCtg.Definition, Value = slc };
                            this.Entity.Categories.Add(ectg);
                        }
                        else {
                            ectg.Value = slc;
                        }
                    }

                }
                this.Entity.RefreshIsRated();
                this.Entity.RefreshUiCue();
            }
        }

        #endregion

    }

    #region Support Classes

    /// <summary>
    /// Visual wrapper for the Rating Entry for media file
    /// </summary>
    public class RatingEntry : NotifyPropertyChangedBase {
        ///<summary>Rating definition</summary>
        private RatingDefinition _definition;
        ///<summary>Rating value</summary>
        private int? _mark;

        ///<summary>Rating value</summary>
        public int? Mark {
            get { return this._mark; }
            set {
                if (this._mark != value) {
                    this._mark = value;
                    this.FirePropertyChanged("Mark");
                }
            }
        }


        ///<summary>Rating definition</summary>
        public RatingDefinition Definition {
            get { return this._definition; }
            set {
                if (this._definition != value) {
                    this._definition = value;
                    this.FirePropertyChanged("Definition");
                }
            }
        }

    }

    /// <summary>
    /// Category item set picker
    /// </summary>
    public class CtgPicker {
        ///<summary>Category Definition</summary>
        private CategoryDefinition _definition;
        ///<summary>Items</summary>
        private List<MarkerWrap<bool, string>> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtgPicker"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public CtgPicker(CategoryDefinition source) {
            this._definition = source;
            if (source.Values != null) {
                this._items = new List<MarkerWrap<bool, string>>();
                foreach (var itm in from ci in source.Values orderby ci select ci) {
                    this._items.Add(new MarkerWrap<bool, string>(itm, false));
                }
            }
        }

        ///<summary>Items</summary>
        public List<MarkerWrap<bool, string>> Items {
            get { return this._items; }
            protected set { this._items = value; }
        }


        ///<summary>Category Definition</summary>
        public CategoryDefinition Definition {
            get { return this._definition; }
            //set {
            //    //if (this._definition != value) {
            //        this._definition = value;
            //    //    this.FirePropertyChanged("definition");
            //    //}
            //}
        }

        /// <summary>
        /// Clears the selection.
        /// </summary>
        public void ClearSelection() {
            if (this.Items != null) {
                foreach (var itm in this.Items)
                    itm.Marker = false;
            }
        }

        /// <summary>
        /// Sets the selected.
        /// </summary>
        /// <param name="src">The source.</param>
        public void SetSelected(IEnumerable<string> src) {
            if ((this.Items != null) && (src != null)) {
                foreach (var it in src) {
                    var mit = this.Items.FirstOrDefault((r) => string.Compare(r.Data, it) == 0);
                    if (mit != null)
                        mit.Marker = true;
                }
            }
        }

        /// <summary>
        /// Enumerates the selected items.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> EnumerateSelected() {
            if (this.Items != null) {
                foreach (var mit in this.Items) {
                    if (mit.Marker)
                        yield return mit.Data;
                }
            }
        }

    }
    #endregion

    
}

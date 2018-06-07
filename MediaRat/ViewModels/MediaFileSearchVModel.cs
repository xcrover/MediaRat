using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {

    ///<summary>View Model for Media File Search</summary>
    public class MediaFileSearchVModel : WorkspaceViewModel {
        ///<summary>Media Project</summary>
        private MediaProject _project;
        ///<summary>Media File search criteria</summary>
        private MediaFileSearchCriteria _criteria= new MediaFileSearchCriteria();
        /////<summary>Entity ratings</summary>
        //private ObservableCollection<RatingEntry> _ratings;
        ///<summary>Category Pickers</summary>
        private ObservableCollection<CategoryFilter> _categories;
        /////<summary>Is Search Mode</summary>
        //private bool _isSearchMode;

        /////<summary>Is Search Mode</summary>
        //public bool IsSearchMode {
        //    get { return this._isSearchMode; }
        //    set {
        //        if (this._isSearchMode != value) {
        //            this._isSearchMode = value;
        //            this.FirePropertyChanged("IsSearchMode");
        //        }
        //    }
        //}
        


        #region Properties
        ///<summary>Media File search criteria</summary>
        public MediaFileSearchCriteria Criteria {
            get { return this._criteria; }
            set {
                if (this._criteria != value) {
                    this._criteria = value;
                    this.FirePropertyChanged("Criteria");
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


        /////<summary>Entity ratings</summary>
        //public ObservableCollection<RatingEntry> Ratings {
        //    get { return this._ratings; }
        //    set {
        //        if (this._ratings != value) {
        //            this._ratings = value;
        //            this.FirePropertyChanged("Ratings");
        //        }
        //    }
        //}

        ///<summary>Category Pickers</summary>
        public ObservableCollection<CategoryFilter> Categories {
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
        /////<summary>Command VModels</summary>
        //private ObservableCollection<CommandVModel> _commandVModels;
        ///<summary>Search Command</summary>
        private RelayCommand _searchCmd;
        ///<summary>Move to the next item Command</summary>
        private RelayCommand _nextCmd;
        ///<summary>Release filter Command</summary>
        private RelayCommand _releaseCmd;

        ///<summary>Release filter Command</summary>
        public RelayCommand ReleaseCmd {
            get { return this._releaseCmd; }
            set { this._releaseCmd = value; }
        }
        

        ///<summary>Move to the next item Command</summary>
        public RelayCommand NextCmd {
            get { return this._nextCmd; }
            set { this._nextCmd = value; }
        }
        

        ///<summary>Search Command</summary>
        public RelayCommand SearchCmd {
            get { return this._searchCmd; }
            set { this._searchCmd = value; }
        }

 
        /////<summary>Command VModels</summary>
        //public ObservableCollection<CommandVModel> CommandVModels {
        //    get { return this._commandVModels; }
        //    set {
        //        if (this._commandVModels != value) {
        //            this._commandVModels = value;
        //            this.FirePropertyChanged("CommandVModels");
        //        }
        //    }
        //}

        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaFileSearchVModel"/> class.
        /// </summary>
        public MediaFileSearchVModel()
            : this("Search") {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaFileSearchVModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public MediaFileSearchVModel(string title) {
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
  
        /// <summary>
        /// Enumerate all the available commands
        /// </summary>
        IEnumerable<RelayCommand> EnumerateCommands() {
            yield return this.SearchCmd;
            yield return this.NextCmd;
            yield return this.ReleaseCmd;
        }

        /// <summary>
        /// Initialize commands infrastructure
        /// </summary>
        void InitCommands() {
            //ObservableCollection<CommandVModel> cmdVms = new ObservableCollection<CommandVModel>();
            //// cmdVms.Add(new CommandVModel(searchCmd) { Name = "searchCmd", Description = "Search Command" });
            //cmdVms.Add(new CommandVModel(ExitCommand));
            ////cmdVms.Add(new CommandVModel(ClonetCmd) { Name = "Clone", Description = "Clone workspace" });
            //CommandVModels = cmdVms;
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
        /// Resets the definitions.
        /// </summary>
        public void ResetDefinitions() {
            ObservableCollection<CategoryFilter> categories = new ObservableCollection<CategoryFilter>();
            ObservableCollection<RatingFilter> ratings = new ObservableCollection<RatingFilter>();
            if (this.Project != null) {
                if (this.Project.RatingDefinitions != null) {
                    var existing = this.Criteria.Ratings;
                    foreach (var q in from rd in this.Project.RatingDefinitions orderby rd.Title select rd) {
                        ratings.Add(new RatingFilter() {
                            Definition = q
                        });
                    }
                    if (existing != null) {
                        foreach (var xrf in existing) {
                            var nrf = ratings.FirstOrDefault((r) => r.Definition == xrf.Definition);
                            if (nrf != null) {
                                nrf.IsActive = xrf.IsActive;
                                nrf.MaxRating = xrf.MaxRating;
                                nrf.MinRating = xrf.MinRating;
                            }
                        }
                    }
                }

                if (this.Project.CategoryDefinitions != null) {
                    foreach (var q in from cd in this.Project.CategoryDefinitions orderby cd.Title select cd) {
                        categories.Add(new CategoryFilter(q));
                    }
                }
            }
            this.RunOnUIThread(() => {
                this.Criteria.Ratings.Clear();
                ratings.Apply((rf) => this.Criteria.Ratings.Add(rf));
                this.Categories = categories;
                this.UpdateView();
            });
        }

        /// <summary>
        /// Updates the criteria.
        /// </summary>
        public void UpdateCriteria() {
            this.Criteria.FilterDelegates.Clear();
            if (this.Categories != null) {
                foreach (var vCtg in this.Categories) {
                    this.Criteria.FilterDelegates.Add(vCtg.GetFilter());
                }
            }
        }

        /// <summary>
        /// Updates the view.
        /// </summary>
        public void UpdateView() {
            //if (this.Categories != null) {
            //    foreach (var ctg in this.Categories) ctg.ClearSelection();
            //    if (this.Criteria.Categories != null) {
            //        foreach (var ectg in this.Criteria.Categories) {
            //            var vCtg = this.Categories.FirstOrDefault((t) => t.Definition == ectg.Key);
            //            if (vCtg != null)
            //                vCtg.SetSelected(ectg.Value);
            //        }
            //    }
            //}
        }
        #endregion
    }

    
}

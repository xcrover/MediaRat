using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {

    ///<summary>View Model for Video Project Track Group</summary>
    public class TrackGroupVModel : WorkspaceViewModel {
        ///<summary>Track Group</summary>
        private MediaTrackGroup _entity;
        ///<summary>View access</summary>
        private ITrackGroupView _viewUtil;
        ///<summary>Current Track</summary>
        private IMediaTrack _currentTrack;

        ///<summary>Current Track</summary>
        public IMediaTrack CurrentTrack {
            get { return this._currentTrack; }
            set {
                if (this._currentTrack != value) {
                    this._currentTrack = value;
                    this.FirePropertyChanged("CurrentTrack");
                    this.ResetViewState();
                }
            }
        }
        

        ///<summary>View access</summary>
        public ITrackGroupView ViewUtil {
            get { return this._viewUtil; }
            set {
                if (this._viewUtil != value) {
                    this._viewUtil = value;
                    this.FirePropertyChanged("ViewUtil");
                }
            }
        }
        

        ///<summary>Track Group</summary>
        public MediaTrackGroup Entity {
            get { return this._entity; }
            set {
                if (this._entity != value) {
                    this._entity = value;
                    this.FirePropertyChanged("Entity");
                }
            }
        }
        

        #region Commands
        ///<summary>Command VModels</summary>
        private ObservableCollection<CommandVModel> _commandVModels;
        ///<summary>Exit Command</summary>
        private RelayCommand _exitCmd;
        ///<summary>Remove selected tracks Command</summary>
        private RelayCommand _removeCmd;
        ///<summary>Move Command</summary>
        private RelayCommand _moveCmd;

        ///<summary>Move Command</summary>
        public RelayCommand MoveCmd {
            get { return this._moveCmd; }
        }

 
        ///<summary>Remove selected tracks Command</summary>
        public RelayCommand RemoveCmd {
            get { return this._removeCmd; }
        }


        ///<summary>Exit Command</summary>
        public RelayCommand ExitCommand {
            get { return this._exitCmd; }
        }

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

        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackGroupVModel"/> class.
        /// </summary>
        public TrackGroupVModel()
            : this("TrackGroupVModel") {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackGroupVModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public TrackGroupVModel(string title) {
            this.Title = title;
            Init();
            this.InitCommands();
        }

        /// <summary>
        /// Initialize business content.
        /// This method can be called more than once
        /// </summary>
        void Init() {
            this.Status= new StatusVModel();
        }
        #endregion

        #region Command plumbing
        void DoExit(object prm) {
            this.OnRequestClose();
        }

        ///<summary>Execute Remove selected tracks Command</summary>
        void DoRemoveCmd(object prm = null) {
            this.Entity.Tracks.Remove(this.CurrentTrack);
        }

        ///<summary>Check if Remove selected tracks Command can be executed</summary>
        bool CanRemoveCmd(object prm = null) {
            return this.CurrentTrack != null;
        }

        ///<summary>Execute Move Command</summary>
        void DoMoveCmd(object prm = null) {
            string tmp = (prm == null) ? string.Empty : prm.ToString().ToLower();
            var track = this.CurrentTrack;
            var tracks = this.Entity.Tracks;
            int ix = tracks.FirstIndex((t) => t == track);
            if (ix >= 0) {
                switch (tmp) {
                    case "top":
                        if (ix > 0) {
                            tracks.RemoveAt(ix);
                            tracks.Insert(0, track);
                        }
                        break;
                    case "up":
                        if (ix > 0) {
                            tracks.RemoveAt(ix);
                            tracks.Insert(ix - 1, track);
                        }
                        break;
                    case "down":
                        if ((ix + 1) < tracks.Count) {
                            tracks.RemoveAt(ix);
                            tracks.Insert(ix + 1, track);
                        }
                        break;
                    case "bottom":
                        if ((ix + 1) < tracks.Count) {
                            tracks.RemoveAt(ix);
                            tracks.Add(track);
                        }
                        break;
                }
                this.CurrentTrack = track;
            }
        }

        ///<summary>Check if Move Command can be executed</summary>
        bool CanMoveCmd(object prm = null) {
            return this.CurrentTrack!=null;
        }

        /// <summary>
        /// Enumerate all the available commands
        /// </summary>
        IEnumerable<RelayCommand> EnumerateCommands() {
            yield return this.MoveCmd;
            yield return this.RemoveCmd;
            yield return this.ExitCommand;
        }

        /// <summary>
        /// Initialize commands infrastructure
        /// </summary>
        void InitCommands() {
            this._moveCmd = new RelayCommand(UIOperations.Move, DoMoveCmd, CanMoveCmd);
            this._removeCmd = new RelayCommand(UIOperations.DeleteItem, DoRemoveCmd, CanRemoveCmd);
            this._exitCmd = new RelayCommand(UIOperations.Exit, DoExit, (p) => true);

            ObservableCollection<CommandVModel> cmdVms = new ObservableCollection<CommandVModel>();
            cmdVms.Add(new CommandVModel(RemoveCmd) { Name = "Remove", Description = "Remove selected tracks from the group" });
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

        #region View callback

        public bool TryAddTrack(IMediaTrack track) {
            if (this.Entity.Tracks.Contains(track)) {
                this.Status.SetError(string.Format("Track '{0}' already exists in this group.", track.Title));
                return false;
            }
            else {
                this.Entity.Tracks.Add(track);
                this.Status.SetPositive(string.Format("Track '{0}' has been added to the group.", track.Title));
                return true;
            }
        }

        /// <summary>
        /// Call back contract for View
        /// </summary>
        public interface ITrackGroupView {

            /// <summary>
            /// Enumerates the selected tracks.
            /// </summary>
            /// <returns></returns>
            IEnumerable<IMediaTrack> EnumerateSelectedTracks();

        }

        #endregion
    }

    
}

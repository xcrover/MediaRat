using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {

    /// <summary>
    /// Search Criteria Filter for Media Files
    /// </summary>
    public interface IMediaFileFilter {
        /// <summary>
        /// Determines whether the specified source is matching.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        bool IsMatching(MediaFile source);
    }

    public class FilterCollection : IMediaFileFilter {
        public List<Func<MediaFile, bool>> Filters = new List<Func<MediaFile, bool>>();
        /// <summary>
        /// Use AND rule for included filters
        /// </summary>
        public bool UseAndRule { get; set; }

        /// <summary>
        /// Determines whether the specified source is matching.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public bool IsMatching(MediaFile source) {
            if (this.UseAndRule) {
                foreach (var flt in this.Filters) {
                    if (!flt(source)) return false;
                }
                return true;
            }
            else {
                foreach (var flt in this.Filters) {
                    if (flt(source)) return true;
                }
                return false;
            }
        }
    }


    /// <summary>
    /// Search criteria for media file inside project
    /// </summary>
    public class MediaFileSearchCriteria : NotifyPropertyChangedBase, IMediaFileFilter {
        ///<summary>Title</summary>
        private string _title;
        ///<summary>Is rated</summary>
        private bool? _isRated;
        ///<summary>Is marked</summary>
        private bool? _isMarked;
        ///<summary>Is Highlighted</summary>
        private bool? _isHighlighted;
        ///<summary>Assigned categories</summary>
        private KeyValuePairXCol<CategoryDefinition, HashSet<string>> _categories = new KeyValuePairXCol<CategoryDefinition, HashSet<string>>();
        ///<summary>Rating filter</summary>
        private ObservableCollection<RatingFilter> _ratings= new ObservableCollection<RatingFilter>();
        ///<summary>Highlighted media</summary>
        private HashSet<MediaFile> _highlightedItems= new HashSet<MediaFile>();
        ///<summary>Marker filter</summary>
        private string _marker;
        ///<summary>Filter functions</summary>
        private List<Func<MediaFile, bool>> _filterDelegates= new List<Func<MediaFile,bool>>();
        ///<summary>Image EXIF filter</summary>
        private ImageExifSearchCriteria _imgExifFilter= new ImageExifSearchCriteria();
        ///<summary>Tech description</summary>
        private string _techDescription;
        ///<summary>Include images</summary>
        private bool? _includeImages;
        ///<summary>Include Video</summary>
        private bool? _includeVideo;

        ///<summary>Include Video</summary>
        public bool? IncludeVideo {
            get { return this._includeVideo; }
            set {
                if (this._includeVideo != value) {
                    this._includeVideo = value;
                    this.FirePropertyChanged(nameof(IncludeVideo));
                }
            }
        }


        ///<summary>Include images</summary>
        public bool? IncludeImages {
            get { return this._includeImages; }
            set {
                if (this._includeImages != value) {
                    this._includeImages = value;
                    this.FirePropertyChanged(nameof(IncludeImages));
                }
            }
        }


        ///<summary>Tech description</summary>
        public string TechDescription {
            get { return this._techDescription; }
            set {
                if (this._techDescription != value) {
                    this._techDescription = value;
                    this.FirePropertyChanged(nameof(TechDescription));
                }
            }
        }


        ///<summary>Image EXIF filter</summary>
        public ImageExifSearchCriteria ImgExifFilter {
            get { return this._imgExifFilter; }
            set { this._imgExifFilter = value; }
        }


        ///<summary>Filter functions</summary>
        public List<Func<MediaFile, bool>> FilterDelegates {
            get { return this._filterDelegates; }
            //set { this._filterDelegates = value; }
        }
        

        ///<summary>Highlighted media</summary>
        public HashSet<MediaFile> HighlightedItems {
            get { return this._highlightedItems; }
            //set {
            //    if (this._highlightedItems != value) {
            //        this._highlightedItems = value;
            //        this.FirePropertyChanged("highlightedItems");
            //    }
            //}
        }
        

        ///<summary>Rating filter</summary>
        public ObservableCollection<RatingFilter> Ratings {
            get { return this._ratings; }
            //set { this._ratings = value; }
        }
        

        ///<summary>Is rated</summary>
        public bool? IsRated {
            get { return this._isRated; }
            set {
                if (this._isRated != value) {
                    this._isRated = value;
                    this.FirePropertyChanged("IsRated");
                }
            }
        }

        ///<summary>Is marked</summary>
        public bool? IsMarked {
            get { return this._isMarked; }
            set {
                if (this._isMarked != value) {
                    this._isMarked = value;
                    this.FirePropertyChanged("IsMarked");
                }
            }
        }


        ///<summary>Is Highlighted</summary>
        public bool? IsHighlighted {
            get { return this._isHighlighted; }
            set {
                if (this._isHighlighted != value) {
                    this._isHighlighted = value;
                    this.FirePropertyChanged("IsHighlighted");
                }
            }
        }
        
       
        ///<summary>Title</summary>
        public string Title {
            get { return this._title; }
            set {
                if (this._title != value) {
                    this._title = value;
                    this.FirePropertyChanged("Title");
                }
            }
        }

        ///<summary>Marker filter</summary>
        public string Marker {
            get { return this._marker; }
            set {
                if (this._marker != value) {
                    this._marker = value;
                    this.FirePropertyChanged("Marker");
                }
            }
        }
        
        ///<summary>Assigned categories</summary>
        public KeyValuePairXCol<CategoryDefinition, HashSet<string>> Categories {
            get { return this._categories; }
            //set { this._categories = value; }
        }

        /// <summary>
        /// Determines whether the specified source is matching.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <returns></returns>
        public bool IsMatching(MediaFile src) {
            if (this.IsRated.HasValue) {
                if (this.IsRated.Value != src.IsRated) return false;
            }
            if (this.IsMarked.HasValue) {
                if (this.IsMarked.Value != src.IsMarked) return false;
            }
            if (this.IsHighlighted.HasValue) {
                if (this.IsHighlighted != this.HighlightedItems.Contains(src)) return false;
            }
            if (!string.IsNullOrEmpty(this.Title)) {
                if (this.Title.StartsWith("\\")&&(src.FullName.IndexOf(this.Title.Substring(1), StringComparison.CurrentCultureIgnoreCase) < 0)) {
                    return false;
                }
                else if (src.Title.IndexOf(this.Title, StringComparison.CurrentCultureIgnoreCase) < 0)
                    return false;
            }
            if (!string.IsNullOrEmpty(this.Marker)&&(!string.IsNullOrEmpty(src.Marker))) {
                if (src.Marker.IndexOf(this.Marker, StringComparison.CurrentCultureIgnoreCase) < 0)
                    return false;
            }

            foreach (var rf in this.Ratings) {
                if (rf.IsActive) {
                    var xr = (src.Ratings == null) ? null : src.Ratings.FirstOrDefault((r) => r.Key == rf.Definition);
                    if (xr == null) {
                        return false;
                    }
                    else {
                        if (!rf.IsMatching(xr.Value)) return false;
                    }
                }
            }
            foreach (var cf in this.Categories) {
                if (cf.Value.Count == 0) continue;
                var xcf = (src.Categories == null) ? null : src.Categories.FirstOrDefault((r) => r.Key == cf.Key);
                if (xcf == null) return false;
                if (!cf.Value.IsSubsetOf(xcf.Value)) return false;
            }
            if (!this.ImgExifFilter.IsMatching(src)) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Build the matching filter.
        /// </summary>
        /// <returns></returns>
        public IMediaFileFilter GetFilter() {
            FilterCollection rz = new FilterCollection() { UseAndRule = true };
            if (this.IsRated.HasValue) {
                rz.Filters.Add((mf) => mf.IsRated == this.IsRated.Value);
            }
            if (this.IsMarked.HasValue) {
                rz.Filters.Add((mf) => mf.IsMarked == this.IsMarked.Value);
            }
            if (this.IsHighlighted.HasValue) {
                rz.Filters.Add((mf) => this.IsHighlighted == this.HighlightedItems.Contains(mf));
            }
            if (!string.IsNullOrEmpty(this.Title)) {
                if (this.Title.StartsWith("\\")) {
                    string toFind = this.Title.Substring(1);
                    rz.Filters.Add((mf) => mf.FullName.IndexOf(toFind, StringComparison.CurrentCultureIgnoreCase) >= 0);
                }
                else {
                    rz.Filters.Add((mf) => mf.Title.IndexOf(this.Title, StringComparison.CurrentCultureIgnoreCase) >= 0);
                }
            }
            if (!string.IsNullOrEmpty(this.Marker)) {
                rz.Filters.Add((mf) => !string.IsNullOrEmpty(mf.Marker) && mf.Marker.IndexOf(this.Marker, StringComparison.CurrentCultureIgnoreCase) >= 0);
            }
            if (!string.IsNullOrEmpty(this.TechDescription)) {
                var flts = this.TechDescription.Split('&');
                string toFind;
                foreach(var fl in flts) {
                    toFind = fl.Trim();
                    if (!string.IsNullOrEmpty(toFind)) {
                        rz.Filters.Add((mf) => !string.IsNullOrEmpty(mf.TechDescription) && mf.TechDescription.IndexOf(toFind, StringComparison.CurrentCultureIgnoreCase) >= 0);
                    }
                }
            }
            if (this.IncludeImages.HasValue) {
                if (this.IncludeImages.Value) {
                    rz.Filters.Add((mf) => mf.MediaType == MediaTypes.Image);
                }
                else {
                    rz.Filters.Add((mf) => mf.MediaType != MediaTypes.Image);
                }
            }
            if (this.IncludeVideo.HasValue) {
                if (this.IncludeVideo.Value) {
                    rz.Filters.Add((mf) => mf.MediaType == MediaTypes.Video);
                }
                else {
                    rz.Filters.Add((mf) => mf.MediaType != MediaTypes.Video);
                }
            }
            foreach (var rf in this.Ratings) {
                if (rf.IsActive)
                    rz.Filters.Add(rf.IsMatching);
            }
            if (!this.ImgExifFilter.IsEmpty) {
                rz.Filters.Add(this.ImgExifFilter.IsMatching);
            }
            if (this.FilterDelegates.Count > 0) {
                rz.Filters.AddRange(this.FilterDelegates);
            }
            return rz;
        }

    }

    /// <summary>
    /// Filter for rating
    /// </summary>
    public class RatingFilter : NotifyPropertyChangedBase, IMediaFileFilter {
        ///<summary>Rating Definition</summary>
        private RatingDefinition _definition;
        ///<summary>Is active</summary>
        private bool _isActive;
        ///<summary>Min rating</summary>
        private int? _minRating;
        ///<summary>Max rating</summary>
        private int? _maxRating;

        ///<summary>Max rating</summary>
        public int? MaxRating {
            get { return this._maxRating; }
            set {
                if (this._maxRating != value) {
                    this._maxRating = value;
                    this.FirePropertyChanged("MaxRating");
                }
            }
        }
        

        ///<summary>Min rating</summary>
        public int? MinRating {
            get { return this._minRating; }
            set {
                if (this._minRating != value) {
                    this._minRating = value;
                    this.FirePropertyChanged("MinRating");
                }
            }
        }

        /// <summary>
        /// Determines whether the specified rating is matching.
        /// </summary>
        /// <param name="rating">The rating.</param>
        /// <returns></returns>
        public bool IsMatching(int rating) {
            if ((this.MinRating.HasValue) && (rating < this.MinRating.Value)) return false;
            if ((this.MaxRating.HasValue) && (rating > this.MaxRating.Value)) return false;
            return true;
        }

        ///<summary>Is active</summary>
        public bool IsActive {
            get { return this._isActive; }
            set {
                if (this._isActive != value) {
                    this._isActive = value;
                    this.FirePropertyChanged("IsActive");
                }
            }
        }
        

        ///<summary>Rating Definition</summary>
        public RatingDefinition Definition {
            get { return this._definition; }
            set { this._definition = value; }
        }


        #region IMediaFileFilter Members

        /// <summary>
        /// Determines whether the specified source is matching.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public bool IsMatching(MediaFile source) {
            if (this.IsActive) {
                var xr = (source.Ratings == null) ? null : source.Ratings.FirstOrDefault((r) => r.Key == this.Definition);
                if (xr == null) {
                    return false;
                }
                else {
                    return this.IsMatching(xr.Value);
                }
            }
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Search filter for categories
    /// </summary>
    public class CategoryFilter : CtgPicker, INotifyPropertyChanged {
        ///<summary>Negate flag</summary>
        private bool _useNegate;
        ///<summary>Use AND rule, i.e. all the medifile must have all the selected categories</summary>
        private bool _useAndRule;

        ///<summary>Use AND rule</summary>
        public bool UseAndRule {
            get { return this._useAndRule; }
            set {
                if (this._useAndRule != value) {
                    this._useAndRule = value;
                    this.FirePropertyChanged("UseAndRule");
                }
            }
        }
        
        ///<summary>Negate flag</summary>
        public bool UseNegate {
            get { return this._useNegate; }
            set {
                if (this._useNegate != value) {
                    this._useNegate = value;
                    this.FirePropertyChanged("UseNegate");
                }
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryFilter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public CategoryFilter(CategoryDefinition source) : base(source) { }

        /// <summary>
        /// Gets the filter.
        /// </summary>
        /// <returns></returns>
        public Func<MediaFile, bool> GetFilter() {
            CtgFilter rz= new CtgFilter(this);
            return rz.IsMatching;
        }

        #region INotifyPropertyChanged
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fires the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public virtual void FirePropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion


    }

    public class CtgFilter : IMediaFileFilter {
        public CategoryDefinition Definition { get; set; }
        public HashSet<string> Categories { get; set; }
        public bool UseNegate { get; set; }
        public bool UseAndRule { get; set; }

        public CtgFilter() { }

        public CtgFilter(CategoryFilter cfg) {
            this.Definition = cfg.Definition;
            this.Categories = new HashSet<string>(cfg.EnumerateSelected());
            this.UseNegate = cfg.UseNegate;
            this.UseAndRule = cfg.UseAndRule;
        }


        #region IMediaFileFilter Members

        public bool IsMatching(MediaFile source) {
            if ((Categories == null) || (Categories.Count == 0)) return true; // Filter is not active
            var mfCtg = (source.Categories == null) ? null : source.Categories.FirstOrDefault((kv) => kv.Key == this.Definition);
            bool ctgMatch;
            if (mfCtg == null) {
                ctgMatch = false;
            }
            else {
                ctgMatch = this.UseAndRule ?
                    this.Categories.IsSubsetOf(mfCtg.Value) : // All requested categories must be present
                    mfCtg.Value.Overlaps(this.Categories); // At least one of the requested categories present
            }
            return this.UseNegate ? !ctgMatch : ctgMatch;
        }

        #endregion
    }


    public class ImageExifSearchCriteria : NotifyPropertyChangedBase, IMediaFileFilter {
        ///<summary>Is vertical image</summary>
        private bool? _isVerticalImg;
        ///<summary>Is Empty</summary>
        private bool _isEmpty;
        ///<summary>ISO Value filter</summary>
        private MinMaxFilter<uint> _isoFilter;

        ///<summary>ISO Value filter</summary>
        public MinMaxFilter<uint> IsoFilter {
            get { return this._isoFilter; }
            set {
                if (this._isoFilter != value) {
                    this._isoFilter = value;
                    this.FirePropertyChanged("IsoFilter");
                }
            }
        }



        ///<summary>Is Empty</summary>
        public bool IsEmpty {
            get { return this._isEmpty; }
            set {
                if (this._isEmpty != value) {
                    this._isEmpty = value;
                    this.FirePropertyChanged("IsEmpty");
                }
            }
        }


        ///<summary>Is vertical image</summary>
        public bool? IsVerticalImg {
            get { return this._isVerticalImg; }
            set {
                if (this._isVerticalImg != value) {
                    this._isVerticalImg = value;
                    this.FirePropertyChanged("IsVerticalImg");
                    Prepare();
                }
            }
        }

        public ImageExifSearchCriteria() {
            this.IsoFilter = new MinMaxFilter<uint>() { Label = "ISO", Description = "Range of ISO values" };
            this.IsoFilter.PropertyChanged += (o,e)=>Prepare();
            Prepare();
        }

        /// <summary>
        /// Prepare filter for usage
        /// </summary>
        public void Prepare() {
            this.IsEmpty= !this.IsVerticalImg.HasValue
                && this.IsoFilter.IsEmpty;
        }

        public bool IsMatching(MediaFile src) {
            if (this.IsEmpty) return true;
            if (src.MediaType != MediaTypes.Image) return false;
            string fpath = src.GetFullPath();
            ExifReader xrf = null;
            ImageDim dm=null;
            UInt32? unz;
            try {
                xrf = new ExifReader(fpath);
                if (this.IsVerticalImg.HasValue) {
                    dm = MediaUtil.GetImgDim(xrf);
                    if (this.IsVerticalImg.Value != dm.IsVert)
                        return false;
                }
                if (!IsoFilter.IsEmpty) {
                    if (null!=(unz = MediaUtil.GetIso(xrf))) {
                        if (!this.IsoFilter.IsMatch(unz.Value)) {
                            return false;
                        }
                    }
                    else {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception x) {
                x.Data["target"] = fpath;
                AppContext.Current.LogTechError(x.ToShortMsg("Read EXIF"), x);
            }
            finally {
                if (xrf != null)
                    xrf.Dispose();
            }
            return false;
        }
    }
}

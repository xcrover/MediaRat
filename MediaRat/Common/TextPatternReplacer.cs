using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace Ops.NetCoe.LightFrame {

    /// <summary>
    /// Text transformer
    /// </summary>
    public interface ITextTransformer {
        /// <summary>
        /// OPTIONAL parameter value retriever for situations when parameter evaluation is costly.
        /// If <c>null</c> or return <c>null</c> then parameter considered not found.
        /// Cacheing of the evluated parameters is up to interface implementation.
        /// </summary>
        Func<string, string> FindParamValue { get; set; }

        /// <summary>
        /// Transforms the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        string Transform(string source);

        /// <summary>
        /// Sets the parameter. This method is used for sitauations when parameter set is already known.
        /// If during the evaluation parameter is not found then <see cref="FindParamValue"/> is called.
        /// </summary>
        /// <param name="prmName">Name of the parameter.</param>
        /// <param name="prmValue">The parameter value.</param>
        void SetParameter(string prmName, string prmValue);
    }

    public class TextStencil : ITextTransformer {
        Func<string, string> _findParamValue;
        //Dictionary<string, string> _predefined;
        TextPatternReplacer _replacer;

        public TextStencil() {
            //_predefined = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            _replacer = new TextPatternReplacer(false, true, false);
            _replacer.GetValue = this.GetKeyVal;
        }

        public Func<string, string> FindParamValue {
            get { return this._findParamValue; }
            set { this._findParamValue = value; }
        }

        /// <summary>
        /// Gets the clean key with stripped brackets.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetCleanKey(string key) {
            if (key.StartsWith("{?") && key.EndsWith("}")) {
                return key.Substring(2, key.Length - 3).Trim();
            }
            return key;
        }

        /// <summary>
        /// Gets the key with brackets.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetDirtyKey(string key) {
            if (key.StartsWith("{?") && key.EndsWith("}")) {
                return key;
            }
            return string.Concat("{?", key, "}");
        }


        string GetKeyVal(string key) {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            string val=null;
            //if (this._predefined.TryGetValue(key, out val))
            //    return val;
            if (this._findParamValue!=null) {
                val = this._findParamValue(GetCleanKey(key));
            }
            return val;
        }

        public void SetParameter(string prmName, string prmValue) {
            this._replacer.Cache[GetDirtyKey(prmName)] = prmValue;
        }

        /// <summary>
        /// Transform the source template using predefined parameters
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public string Transform(string source) {
            return this._replacer.Replace(source);
        }
    }

    /// <summary>
    /// Replaces patterns in text using <see cref="Regex"/>
    /// </summary>
    public class TextPatternReplacer {
        #region Private Members
        const string _recursionInProgress = "RecursionInProgress";
        ///<summary>Pattern to replace</summary>
        private string _pattern = @"{\?.+?}";
        ///<summary>Regex</summary>
        private Regex _worker;
        ///<summary>Function that returns value by the key matched to the pattern</summary>
        private Func<string, string> _getValue;
        ///<summary>Determines if result of <see cref="GetValue"/> also must be evaluated recursively.</summary>
        private bool _isRecursive;
        ///<summary>Is cache key Case Sensetive</summary>
        private bool _isCaseSensitive;

        /// <summary>Local cache</summary>
        private Dictionary<string, string> _cache;

        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="TextPatternReplacer"/> class.
        /// </summary>
        public TextPatternReplacer() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextPatternReplacer"/> class.
        /// </summary>
        /// <param name="isRecursive">if set to <c>true</c> is recursive.</param>
        /// <param name="isCaching">if set to <c>true</c> is caching.</param>
        /// <param name="isCaseSensitive">if set to <c>true</c> is cache key case sensitive.</param>
        public TextPatternReplacer(bool isRecursive, bool isCaching = true, bool isCaseSensitive = false) {
            this._isRecursive = isRecursive;
            this._isCaseSensitive = isCaseSensitive;
            this.IsCaching = isCaching;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextPatternReplacer"/> class.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="getValueMethod">The get Value Method.</param>
        /// <param name="isRecursive">if set to <c>true</c> is recursive.</param>
        /// <param name="isCaching">if set to <c>true</c> is caching.</param>
        /// <param name="isCaseSensitive">if set to <c>true</c> is cache key case sensitive.</param>
        public TextPatternReplacer(string pattern, Func<string, string> getValueMethod, bool isRecursive= true, bool isCaching= true, bool isCaseSensitive= false) {
            this._pattern = pattern;
            this._getValue = getValueMethod;
            this._isRecursive = isRecursive;
            this._isCaseSensitive = isCaseSensitive;
            this.IsCaching = isCaching;
        }
        #endregion

        #region Properties
        ///<summary>Pattern to replace. Default value "{\?.+?}", i.e. matches "{?something}"</summary>
        public string Pattern {
            get { return this._pattern; }
            set { this._pattern = value; }
        }

        ///<summary>Regex</summary>
        public Regex Worker {
            get {
                if (this._worker == null)
                    this._worker = new Regex(this.Pattern);
                return this._worker; 
            }
            set { this._worker = value; }
        }

        ///<summary>Function that returns value by the key matched to the pattern</summary>
        public Func<string, string> GetValue {
            get { return this._getValue; }
            set { this._getValue = value; }
        }

        ///<summary>Determines if result of <see cref="GetValue"/> also must be evaluated recursively.</summary>
        public bool IsRecursive {
            get { return this._isRecursive; }
            //protected set { this._isRecursive = value; }
        }

        ///<summary>Is cache key Case Sensetive</summary>
        public bool IsCaseSensitive {
            get { return this._isCaseSensitive; }
            //set { this._isCaseSensitive = value; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether this instance is caching evaluated values.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is caching; otherwise, <c>false</c>.
        /// </value>
        public bool IsCaching {
            get { return this._cache != null; }
            set {
                if (value) {
                    if (this._cache == null) {
                        this._cache = (this.IsCaseSensitive) ? new Dictionary<string, string>() : new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    }
                }
                else {
                    this._cache = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the cache.
        /// </summary>
        /// <value>
        /// The cache.
        /// </value>
        public Dictionary<string, string> Cache {
            get { return this._cache; }
            set { this._cache = value; }
        }

        #endregion

        #region Operations

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public TextPatternReplacer Clone() {
            TextPatternReplacer result = new TextPatternReplacer(this.Pattern, this.GetValue, this.IsRecursive, this.IsCaching, this.IsCaseSensitive) {
                Cache= this.Cache
            };
            if (this._worker != null) {
                result.Worker = new Regex(this.Pattern, this.Worker.Options);
            }
            return result;
        }

        /// <summary>
        /// Replaces the matched substrings in the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>String with patterns replaced with evaluated values.</returns>
        public string Replace(string source) {
            if (string.IsNullOrEmpty(source)) return source;
            return this.Worker.Replace(source, Evaluate);
        }

        /// <summary>
        /// Evaluates the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string Evaluate(string key) {
            string val;
            if (this.IsCaching) {
                if (this.Cache.TryGetValue(key, out val)) {
                    if (object.ReferenceEquals(val, _recursionInProgress)) {
                        throw new ApplicationException(string.Format("Failed to evaluate '{0}': circular dependency detected.", key));
                    }
                    return val;
                }
            }
            val = this.GetValue(key);
            if (this.IsRecursive) {
                this.Cache[key] = _recursionInProgress;
                val = this.Clone().Replace(val);
            }
            if (this.IsCaching) {
                this.Cache[key] = val;
            }
            return val;
        }

        string Evaluate(Match match) {
            return Evaluate(match.ToString());
        }
        #endregion

    }
}

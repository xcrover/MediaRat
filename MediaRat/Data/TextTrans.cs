using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {

    /// <summary>
    /// Text transformation contract
    /// </summary>
    public interface ITextTrans {
        /// <summary>
        /// Gets the keyword for formula.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        string Key { get; }
        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        string ShortName { get; }
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        string Description { get; }
        /// <summary>
        /// Tries the parse.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="evaluator">The evaluator.</param>
        /// <returns></returns>
        bool TryParse(string formula, out Func<MediaFile, string> evaluator);
    }

    /// <summary>
    /// Top-level transformer
    /// </summary>
    public class TextTransformer {
        private List<ITextTrans> _definitions = new List<ITextTrans>();

        /// <summary>
        /// Gets the definitions.
        /// </summary>
        /// <value>
        /// The definitions.
        /// </value>
        public List<ITextTrans> Definitions {
            get { return this._definitions; }
        }

        /// <summary>
        /// Adds the definition.
        /// </summary>
        /// <param name="definition">The definition.</param>
        public void AddDefinition(ITextTrans definition) {
            this._definitions.Add(definition);
        }

        /// <summary>
        /// Gets the evaluator.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        public Func<MediaFile, string> GetEvaluator(string formula) {
            CompositeEval rz = new CompositeEval();
            var subFormulas= formula.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            ITextTrans ttr;
            StringBuilder err = new StringBuilder();
            Func<MediaFile, string> evaluator;
            foreach (var sf in subFormulas) {
                ttr = _definitions.FirstOrDefault((t) => sf.StartsWith(t.Key, StringComparison.InvariantCultureIgnoreCase));
                if (ttr == null) {
                    string tmp= sf;
                    rz._subEvaluators.Add((mf) => tmp);
                }
                else {
                    if (ttr.TryParse(sf, out evaluator)) {
                        rz._subEvaluators.Add(evaluator);
                    }
                    else {
                        err.AppendLine(evaluator(null));
                    }
                }
            }
            if (err.Length > 0) {
                throw new ArgumentException(err.ToString());
            }
            return rz.GetValue;
        }

        class CompositeEval {
            public List<Func<MediaFile, string>> _subEvaluators = new List<Func<MediaFile, string>>();
            public string GetValue(MediaFile mf) {
                StringBuilder sb = new StringBuilder();
                foreach (var ev in this._subEvaluators) {
                    sb.Append(ev(mf));
                }
                return sb.ToString();
            }
        }

    }

    /// <summary>
    /// Simple string parser
    /// </summary>
    public class StrParser {
        string _source;
        int _cp;
        public const char Nil = Char.MinValue;

        /// <summary>
        /// Nexts the character.
        /// </summary>
        /// <param name="rz">The rz.</param>
        /// <returns></returns>
        public bool ReadChar(out char rz) {
            if (_cp < _source.Length) {
                rz = _source[_cp];
                _cp++;
                return true;
            }
            else {
                rz = Char.MinValue;
                return false;
            }
        }

        /// <summary>
        /// Gets the next character.
        /// </summary>
        /// <value>
        /// The next character.
        /// </value>
        public char NextChar {
            get {
                return (_cp < _source.Length) ?
                    _source[_cp] : Nil;
            }
        }

        /// <summary>
        /// Advances this instance one step.
        /// </summary>
        public void Advance() { this._cp++; }

        /// <summary>
        /// Reads the int.
        /// </summary>
        /// <param name="rz">The rz.</param>
        /// <returns></returns>
        public bool ReadInt(out int rz) {
            int bx = _cp;
            while (_cp < _source.Length) {
                if (!Char.IsDigit(this._source[_cp]))
                    break;
                _cp++;
            }
            if (bx < _cp) {
                return int.TryParse(_source.Substring(bx, _cp - bx), out rz);
            }
            else {
                rz = int.MinValue;
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is eol.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is eol; otherwise, <c>false</c>.
        /// </value>
        public bool IsEol {
            get { return this._cp >= this._source.Length; }
        }

        /// <summary>
        /// Reads to end.
        /// </summary>
        /// <returns></returns>
        public string ReadToEnd() {
            if (this._cp < this._source.Length) {
                int ix = _cp;
                _cp = this._source.Length;
                return this._source.Substring(ix);
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Sets the source.
        /// </summary>
        /// <param name="source">The source.</param>
        public void SetSource(string source) {
            this._source = source;
            this._cp = 0;
        }
    }

    /// <summary>
    /// Implementation of <see cref="ITextTrans"/> for subname extractor
    /// </summary>
    public class TTSubName : ITextTrans {

        #region ITextTrans Members

        /// <summary>
        /// Gets the keyword for formula.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public string Key {
            get { return "N"; }
        }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public string ShortName {
            get { return "SubName"; }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public string Description {
            get {
                return "Get the original name or its part\r\n'N' - get the whole name\r\n'NL10' - get left part up to 10 symbols\r\n" +
                    "'NR15' - get right part up to 10 symbols\r\n'N10-30' - get substring from position 10 to 30\r\n"+
                    "'N10-3T' - get right part starting from position 10 and truncate last 3 symbols";
            }
        }

        /// <summary>
        /// Tries the parse.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="evaluator">The evaluator.</param>
        /// <returns></returns>
         public bool TryParse(string formula, out Func<MediaFile, string> evaluator) {
            SubNameEval rz = new SubNameEval();
            rz._error = null;
            evaluator = rz.GetError;
            StrParser prs = new StrParser();
            char ch;
            try {
                prs.SetSource(formula.ToUpper());
                if (!prs.ReadChar(out ch) || (ch != 'N')) {
                    rz._error = string.Format("Error in formaula \"{0}\": must start from 'N'", formula);
                    return false;
                }
                if (prs.IsEol) {
                    evaluator = rz.GetOriginalName;
                    return true;
                }
                ch = prs.NextChar;
                if ((ch == 'L') || (ch == 'R')) { // NL10 or NR10 format
                    prs.Advance();
                    if (prs.ReadInt(out rz._ix)) {
                        if (ch == 'L')
                            evaluator = rz.GetLeft;
                        else
                            evaluator = rz.GetRight;
                        return true;
                    }
                    else {
                        rz._error = string.Format("Error in formaula \"{0}\": must be integer after 'N{1}'", formula, ch);
                        return false;
                    }
                }
                else if (Char.IsDigit(ch)) { // N10-20 format
                    int rx;
                    if (prs.ReadInt(out rz._ix) &&
                        prs.ReadChar(out ch) && (ch == '-') &&
                        prs.ReadInt(out rx)) 
                    {
                        if (Char.ToLower(prs.NextChar) == 't') { // N10-3T (Trim right form)
                            rz._tr = rx;
                            evaluator = rz.GetSubT;
                        }
                        else {
                            rz._len = rx - rz._ix + 1;
                            if (rz._len <= 0) {
                                rz._error = string.Format("Error in formaula \"{0}\": bad requested substring length {1}. Must look like 'N10-20' to get substring from position 10 to 20", formula, rz._len);
                                return false;
                            }
                            evaluator = rz.GetSub;
                        }
                        return true;
                    }
                    else {
                        rz._error = string.Format("Error in formaula \"{0}\": must look like 'N10-20' to get substring from position 10 to 20", formula);
                        return false;
                    }
                }
                rz._error = string.Format("Error in formaula \"{0}\". Must be in form: 'N'|'NLnn'|'NRnn'|'Nnn-nn' where 'nn' is integer", formula);
                return false;
            }
            catch (Exception x) {
                rz._error = string.Format("Error in formaula \"{0}\". {1}: {2}", formula, x.GetType().Name, x.Message);
                return false;
            }
        }


        #endregion

        class SubNameEval {
            public int _ix, _len, _tr;
            public string _error;

            /// <summary>
            /// Gets the name of the original.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            public string GetOriginalName(MediaFile source) {
                return source.Title;
            }

            /// <summary>
            /// Gets the left.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            public string GetLeft(MediaFile source) {
                return source.Title.GetLeft(this._ix);
            }

            /// <summary>
            /// Gets the right.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            public string GetRight(MediaFile source) {
                return source.Title.GetRight(this._ix);
            }

            /// <summary>
            /// Gets the substring.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            public string GetSub(MediaFile source) {
                string src = source.Title;
                if (this._ix >= src.Length)
                    return string.Empty;
                if ((this._ix + this._len - 1) < src.Length)
                    return src.Substring(this._ix, this._len);
                else
                    return src.Substring(this._ix);
            }

            /// <summary>
            /// Gets the substring truncated on right.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            public string GetSubT(MediaFile source) {
                string src = source.Title;
                int rln= src.Length-this._ix-this._tr;
                if (rln<=0)
                    return string.Empty;
                return src.Substring(this._ix, rln);
            }


            /// <summary>
            /// Gets the error.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            public string GetError(MediaFile source) {
                return this._error;
            }

        }
    }

    /// <summary>
    /// Implementation of <see cref="ITextTrans"/> for a simple counter
    /// </summary>
    public class TTCounter : ITextTrans {

        #region ITextTrans Members

        /// <summary>
        /// Gets the keyword for formula.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key {
            get { return "C"; }
        }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public string ShortName {
            get { return "Counter"; }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description {
            get {
                return "Get the counter value\r\n'C' - get the number starting from 1\r\n'C100' - get the numbers starting from 100\r\n" +
                    "'C5F000' - get the numbers starting from 5 using format after 'F' (005, 006 etc.)";
            }
        }

        /// <summary>
        /// Tries the parse.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="evaluator">The evaluator.</param>
        /// <returns></returns>
        public bool TryParse(string formula, out Func<MediaFile, string> evaluator) {
            CounterEval rz = new CounterEval() { _fmt = "d" };
            rz._error = null;
            evaluator = rz.GetError;
            StrParser prs = new StrParser();
            char ch;
            try {
                prs.SetSource(formula.ToUpper());
                if (!prs.ReadChar(out ch) || (ch != 'C')) {
                    rz._error = string.Format("Error in formaula \"{0}\": must start from 'C'", formula);
                    return false;
                }
                if (prs.IsEol) {
                    rz._start = 0;
                    evaluator = rz.GetCounter;
                    return true;
                }
                ch = prs.NextChar;
                if (Char.IsDigit(ch)) { // C10
                    if (!prs.ReadInt(out rz._start)) {
                        rz._error = string.Format("Error in formaula \"{0}\": cannot parse integer after 'C'", formula);
                        return false;
                    }
                    if (prs.IsEol) {
                        evaluator = rz.GetCounter;
                        return true;
                    }
                    ch = prs.NextChar;
                }
                if (ch == 'F') { // CF000 format
                    prs.Advance();
                    rz._fmt = prs.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(rz._fmt)) {
                        rz._error = string.Format("Error in formaula \"{0}\": format must be integer after 'CF'", formula);
                        return false;
                    }
                    else {
                        evaluator = rz.GetCounter;
                        return true;
                    }
                }
                rz._error = string.Format("Error in formaula \"{0}\". Must be in form: 'C'|'Cnn'|'CnnFfmt'|'CFfmt' where 'nn' is integer and fmt is integer format, e.g. 000", formula);
                return false;
            }
            catch (Exception x) {
                rz._error = string.Format("Error in formaula \"{0}\". {1}: {2}", formula, x.GetType().Name, x.Message);
                return false;
            }
        }

 
        #endregion


        class CounterEval {
            public int _start;
            public string _error, _fmt;

            /// <summary>
            /// Gets the name of the original.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            public string GetCounter(MediaFile source) {
                return (++this._start).ToString(this._fmt);
            }

            /// <summary>
            /// Gets the error.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            public string GetError(MediaFile source) {
                return this._error;
            }

        }

    }

    /// <summary>
    /// Implementation of <see cref="ITextTrans"/> for a direct text
    /// </summary>
    public class TTConst : ITextTrans {
        string _text;

        #region ITextTrans Members

        /// <summary>
        /// Gets the keyword for formula.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key {
            get { return "\""; }
        }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public string ShortName {
            get { return "Text"; }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description {
            get {
                return "Get the text value directly. You can use either simple text or start from symbol '\"' to avoind conflicts with other formulas.";
            }
        }

        /// <summary>
        /// Tries the parse.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="evaluator">The evaluator.</param>
        /// <returns></returns>
        public bool TryParse(string formula, out Func<MediaFile, string> evaluator) {
            TextDirectEval rz = new TextDirectEval();
            evaluator = rz.GetText;
            try {
                rz._text = null;
                string txt = formula.Trim();
                int ix=0, ln=0;

                if (txt.StartsWith("\""))
                    ix = 1;
                if (txt.EndsWith("\""))
                    ln = txt.Length - 1 - ix;
                rz._text = txt.Substring(ix, ln);
                return true;
            }
            catch (Exception x) {
                this._text = string.Format("Error in formaula \"{0}\". {1}: {2}", formula, x.GetType().Name, x.Message);
                return false;
            }
        }

        #endregion

        class TextDirectEval {
            public string _text;

            public string GetText(MediaFile source) {
                return this._text;
            }
        }
         
    }

    /// <summary>
    /// Implementation of <see cref="ITextTrans"/> for the media rating
    /// </summary>
    public class TTRating : ITextTrans {
        RatingDefinition _rating;
        string _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="TTRating"/> class.
        /// </summary>
        public TTRating(RatingDefinition rating) {
            this._rating = rating;
            this._key = "@" + rating.Marker;
        }

        #region ITextTrans Members

        /// <summary>
        /// Gets the keyword for formula.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key {
            get { return this._key; }
        }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public string ShortName {
            get { return string.Format("r:{0}", this._rating.Title); }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description {
            get {
                return string.Format("Get the {0} rating value, e.g.\r\n'{1}' will give value '{2}6' for {0} 6.", this._rating.Title, this._key, this._rating.Marker);
            }
        }

        /// <summary>
        /// Tries the parse.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="evaluator">The evaluator.</param>
        /// <returns></returns>
        public bool TryParse(string formula, out Func<MediaFile, string> evaluator) {
            RatingEval rz = new RatingEval() { _marker = this._rating.Marker };
            evaluator = rz.GetRating;
            return true;
        }

        #endregion

        class RatingEval {
            public string _marker;

            public string GetRating(MediaFile source) {
                if (source.Ratings != null) {
                    var rt= source.Ratings.FirstOrDefault((r) => r.Key.Marker == this._marker);
                    if (rt == null)
                        return string.Empty;
                    else
                        return string.Format("{0}{1}", this._marker, rt.Value);
                }
                return string.Empty;
            }
        }

    }

    /// <summary>
    /// Implementation of <see cref="ITextTrans"/> for the media categories
    /// </summary>
    public class TTCategory : ITextTrans {
        CategoryDefinition _category;
        string _separator = "+";
        string _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="TTCategory" /> class.
        /// </summary>
        /// <param name="category">The category definition.</param>
        public TTCategory(CategoryDefinition category) {
            this._category = category;
            this._key = "$" + category.Marker;
        }

        #region ITextTrans Members

        /// <summary>
        /// Gets the keyword for formula.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key {
            get { return this._key; }
        }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public string ShortName {
            get { return string.Format("c:{0}", this._category.Title); }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description {
            get {
                return string.Format("Get the {0} categories, e.g.\r\n'{1}' will give value 'Category1{3}Category2' for {0}.", 
                    this._category.Title, this._key, this._category.Marker, this._separator);
            }
        }

        /// <summary>
        /// Tries the parse.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="evaluator">The evaluator.</param>
        /// <returns></returns>
        public bool TryParse(string formula, out Func<MediaFile, string> evaluator) {
            CategoryEval rz = new CategoryEval() { 
                _marker = this._category.Marker,
                _separator= this._separator
            };
            evaluator = rz.GetCategories;
            return true;
        }

        #endregion

        class CategoryEval {
            public string _marker;
            public string _separator;

            public string GetCategories(MediaFile source) {
                if (source.Categories != null) {
                    var ct = source.Categories.FirstOrDefault((r) => r.Key.Marker == this._marker);
                    if ((ct == null)||(ct.Value==null)||(ct.Value.Count==0))
                        return string.Empty;
                    else {
                        return string.Join(_separator, from s in ct.Value orderby s select s);
                        //return string.Format("[{1}]", 
                        //    this._marker, string.Join(_separator, from s in ct.Value orderby s select s));
                    }
                }
                return string.Empty;
            }
        }

    }


    /// <summary>
    /// Implementation of <see cref="ITextTrans"/> for the media marker
    /// </summary>
    public class TTMarker : ITextTrans {

        /// <summary>
        /// Initializes a new instance of the <see cref="TTRating"/> class.
        /// </summary>
        public TTMarker() {
       }

        #region ITextTrans Members

        /// <summary>
        /// Gets the keyword for formula.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key {
            get { return "M"; }
        }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public string ShortName {
            get { return "Marker"; }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description {
            get {
                return "Get the media file marker value";
            }
        }

        /// <summary>
        /// Tries the parse.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="evaluator">The evaluator.</param>
        /// <returns></returns>
        public bool TryParse(string formula, out Func<MediaFile, string> evaluator) {
            evaluator = GetMarker;
            return true;
        }

        #endregion

        public string GetMarker(MediaFile source) {
            if (string.IsNullOrWhiteSpace(source.Marker)) return string.Empty;
            return source.Marker.Trim();
        }

    }


}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ops.NetCoe.LightFrame {

    /// <summary>
    /// Key:value pair with <see cref="INotifyPropertyChanged"/> implementation.
    /// </summary>
    /// <typeparam name="Tkey"></typeparam>
    /// <typeparam name="Tval"></typeparam>
    public class KeyValuePairX<Tkey, Tval> : NotifyPropertyChangedBase {
        ///<summary>Key</summary>
        private Tkey _key;
        ///<summary>Value</summary>
        private Tval _value;

        ///// <summary>
        ///// Initializes a new instance of the <see cref="KeyValuePairX{Tkey, Tval}"/> class.
        ///// </summary>
        //public KeyValuePairX() {
        //    this._key = default(Tkey);
        //    this._value = default(Tval);
        //}

        ///<summary>Key</summary>
        public Tkey Key {
            get { return this._key; }
            set {
                if (!object.Equals(this._key, value)) {
                    this._key = value;
                    this.FirePropertyChanged("Key");
                }
            }
        }

        ///<summary>Value</summary>
        public Tval Value {
            get { return this._value; }
            set {
                if (!object.Equals(this._value, value)) {
                    this._value = value;
                    this.FirePropertyChanged("Value");
                }
            }
        }
    }

    /// <summary>
    /// Observable collection of Key:Value pairs
    /// </summary>
    /// <typeparam name="Tkey">The type of the key.</typeparam>
    /// <typeparam name="Tval">The type of the val.</typeparam>
    public class KeyValuePairXCol<Tkey, Tval> : ObservableCollection<KeyValuePairX<Tkey, Tval>> {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairXCol{Tkey, Tval}"/> class.
        /// </summary>
        public KeyValuePairXCol() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairXCol{Tkey, Tval}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public KeyValuePairXCol(IEnumerable<KeyValuePairX<Tkey, Tval>> source) : base(source) { }

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The value.</param>
        public void Add(Tkey key, Tval val) {
            base.Add(new KeyValuePairX<Tkey, Tval>() { Key = key, Value = val });
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="source">The source.</param>
        public void AddRange(IEnumerable<KeyValuePairX<Tkey, Tval>> source) {
            if (source != null) {
                foreach (var itm in source)
                    base.Add(itm);
            }
        }
    }



    /// <summary>
    /// Code:Value Pair
    /// </summary>
    public class CodeValuePair {
        ///<summary>Code</summary>
        private string _code;
        ///<summary>Value</summary>
        private string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeValuePair"/> class.
        /// </summary>
        public CodeValuePair() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeValuePair"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="val">The val.</param>
        public CodeValuePair(string code, string val) {
            this._code = code;
            this._value = val;
        }

        ///<summary>Value</summary>
        public string Value {
            get { return this._value; }
            set { this._value = value; }
        }
        

        ///<summary>Code</summary>
        public string Code {
            get { return this._code; }
            set { this._code = value; }
        }
        
    }

    public class CodeValueList : List<CodeValuePair> {
        ///<summary>String comparison</summary>
        private StringComparison _strComparison = StringComparison.OrdinalIgnoreCase;

        ///<summary>String comparison</summary>
        public StringComparison StrComparison {
            get { return this._strComparison; }
            set { this._strComparison = value; }
        }

        public CodeValueList() { }

        public CodeValueList(IEnumerable<CodeValuePair> source) : base(source) { }

        /// <summary>
        /// Try to find pair by code
        /// </summary>
        /// <param name="code"></param>
        /// <param name="rz"></param>
        /// <returns></returns>
        public bool TryFind(string code, out CodeValuePair rz) {
            foreach (var p in this) {
                if (code.Equals(p.Code, this.StrComparison)) {
                    rz = p;
                    return true;
                }
            }
            rz = null;
            return false;
        }

        /// <summary>
        /// If code exists then value is overriden; otherwise new pair is added
        /// </summary>
        /// <param name="code">Code</param>
        /// <param name="val">Value</param>
        /// <returns></returns>
        public CodeValuePair Ensure(string code, string val) {
            CodeValuePair cvp;
            if (TryFind(code, out cvp)) {
                cvp.Value = val;
                return cvp;
            }
            else {
                cvp = new CodeValuePair(code, val);
                this.Add(cvp);
                return cvp;
            }
        }

        /// <summary>
        /// Ensure value if it is not null or empty
        /// </summary>
        /// <param name="code"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public CodeValuePair EnsureIf(string code, object val) {
            if (val != null) {
                var sval = val.ToString();
                if (!string.IsNullOrEmpty(sval))
                    return this.Ensure(code, val.ToString());
            }
            return null;
        }
    }
}

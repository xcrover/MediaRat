using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace XC.MediaRat {
    /// <summary>
    /// Based on http://www.codeproject.com/Articles/3111/C-NET-Command-Line-Arguments-Parser
    /// </summary>
    public class StartupOptions {
        private Dictionary<string, string> _argMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        const string cnDefault = "true";

        public StartupOptions ( ) {
            Parse(Environment.GetCommandLineArgs());
        }

        void RegisterPrm(string key, string val) {
            if (!_argMap.ContainsKey(key))
                _argMap.Add(key, val);
        }

        void Parse(string[] args) {
            Regex splitter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            Regex remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string prmKey = null;
            string[] parts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" 
            //   /param4=happy -param5 '--=nice=--'
            foreach (string itm in args) {
                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)
                parts = splitter.Split(itm, 3);

                switch (parts.Length) {
                    // Found a value (for the last parameter 
                    // found (space separator))
                    case 1:
                        if (prmKey != null) {
                            RegisterPrm(prmKey, parts[0] = remover.Replace(parts[0], "$1"));
                            prmKey = null;
                        }
                        // else Error: no parameter waiting for a value (skipped)
                        break;

                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (prmKey != null) {
                            RegisterPrm(prmKey, cnDefault);
                        }
                        prmKey = parts[1];
                        break;

                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (prmKey != null) {
                            RegisterPrm(prmKey, cnDefault);
                        }

                        prmKey = parts[1];

                        // Remove possible enclosing characters (",')
                        RegisterPrm(prmKey, parts[2] = remover.Replace(parts[2], "$1"));
                        prmKey = null;
                        break;
                }
            }
            // In case a parameter is still waiting
            if (prmKey != null) {
                RegisterPrm(prmKey, cnDefault);
            }
        }

        public string this[string prmKey] {
            get {
                string rz;
                if (this._argMap.TryGetValue(prmKey, out rz))
                    return rz;
                else
                    return null;
            }
        }

    }
}

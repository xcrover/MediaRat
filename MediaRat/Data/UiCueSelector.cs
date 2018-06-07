using Ops.NetCoe.LightFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace XC.MediaRat {
    public class UiCueSelector : IXmlConfigurable {
        public static XName XPrime { get { return XN.xnUiCueSelcter; } }
        ///<summary>Rules</summary>
        private List<UiCueRule> _rules= new List<UiCueRule>();
        public MediaProject Project { get; protected set; }

        ///<summary>Rules</summary>
        public List<UiCueRule> Rules {
            get { return this._rules; }
            protected set { this._rules = value; }
        }

        public UiCueSelector(MediaProject project) {
            this.Project = project;
        }

        public void Init() {
            this.Rules.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        public void Clear() {
            this.Rules.Clear();
        }

        public UiCues GetCue(MediaFile mf, UiCues defaultVal=UiCues.Default) {
            foreach(var r in this.Rules) {
                if (r.Filter.IsMatching(mf))
                    return r.Cue;
            }
            return defaultVal;
        }

        class XN  {
            ///<summary>The XML name for UiCueSelcter [uiCueSelector]</summary>
            public static XName xnUiCueSelcter = XName.Get("uiCueSelector");
            ///<summary>The XML name for Cue [cue]</summary>
            public static XName xaCue = XName.Get("cue");
            ///<summary>The XML name for Field [field]</summary>
            public static XName xaField = XName.Get("field");
            ///<summary>The XML name for Filter [filter]</summary>
            public static XName xaFilter = XName.Get("filter");
            ///<summary>The XML name for UseAND [useAnd]</summary>
            public static XName xaUseAnd = XName.Get("useAnd");
        }

        void CfgError(string errmsg) {
            throw new BizException(errmsg);
        }

        public void ApplyConfiguration(XElement configSource, string password = null) {
            this.Clear();
            var xls = configSource.Elements(XNames.xnItem);
            UiCueRule ucr;
            string rlType;
            foreach (var xf in xls) {
                try {
                    ucr = new UiCueRule() {
                        Order = xf.GetMandatoryAttribute<int>(XNames.xaOrderW, x => (int)x),
                        Cue = xf.GetMandatoryAttribute<UiCues>(XN.xaCue, x => x.Value.ToEnum(UiCues.Default))
                    };
                    rlType = xf.GetAttributeValue(XNames.xaType);
                    switch (rlType) {
                        case "text": ucr.Filter = CreateTextFilter(xf); break;
                        case "ctg": ucr.Filter = CreateCtgFilter(xf); break;
                        default:
                            throw new ArgumentOutOfRangeException(string.Format("UI Cue Selector does not recognize Filter type '{0}'.", rlType));
                    }
                    if (ucr.Filter!=null)
                        this.Rules.Add(ucr);
                }
                catch (BizException bx) {
                    if (!AppContext.Current.GetServiceViaLocator<IUIHelper>().GetUserConfirmation(string.Format("UiCue config error: {0}\r\n\r\n\tContinue?", bx.Message))) {
                        throw new BizException(string.Format("Canceled by user due to UiCue config error: {0}", bx.Message));
                    }
                }
                catch (Exception x) {
                    if (!AppContext.Current.GetServiceViaLocator<IUIHelper>().GetUserConfirmation(string.Format("UiCue config error: {0}\r\n\r\n\tContinue?", x.ToShortMsg()))) {
                        throw new BizException(string.Format("Canceled by user due to UiCue config error: {0}", x.ToShortMsg()));
                    }
                }
            }
            this.Init();
        }

        const string cnFldNames = "Name|Marker|Memo|Tech";
        const string cnRegex = "regex:";

        IMediaFileFilter CreateTextFilter(XElement xcf) {
            
            
            string fld= xcf.GetAttributeValue(XN.xaField, cnFldNames);
            FilterCollection rz = new FilterCollection() {
                UseAndRule = xcf.GetAttributeBool(XN.xaUseAnd)
            };
            Func<string, bool> flt = null;
            var sflt = xcf.GetMandatoryAttribute(XN.xaFilter, CfgError);
            bool negate = xcf.GetAttributeBool(XNames.xaNegate);
            if (sflt.StartsWith(cnRegex, StringComparison.OrdinalIgnoreCase)) {
                var rgx = new Regex(sflt.Substring(cnRegex.Length), RegexOptions.IgnoreCase);
                if (negate)
                    flt = (s) => string.IsNullOrEmpty(s) || !rgx.IsMatch(s);
                else
                    flt= (s) => !string.IsNullOrEmpty(s) && rgx.IsMatch(s);
            }
            else {
                if (negate)
                    flt = (s) => string.IsNullOrEmpty(s) || (s.IndexOf(sflt, StringComparison.InvariantCultureIgnoreCase) < 0);
                else
                    flt = (s) => !string.IsNullOrEmpty(s) && (s.IndexOf(sflt, StringComparison.InvariantCultureIgnoreCase)>=0);
            }

            foreach(var fnm in fld.Split(new char[] { '|', ','}, StringSplitOptions.RemoveEmptyEntries)) {
                switch (fnm.ToLower()) {
                    case "name": rz.Filters.Add(mf => flt(mf.Title)); break;
                    case "marker": rz.Filters.Add(mf => flt(mf.Marker)); break;
                    case "memo": rz.Filters.Add(mf => flt(mf.Description)); break;
                    case "tech": rz.Filters.Add(mf => flt(mf.TechDescription)); break;
                    default:
                        throw new BizException(string.Format("UiCue text filter not supported for field '{0}'. Supported fields: {1}", fnm, cnFldNames));
                }
            }
            return rz;
        }

        IMediaFileFilter CreateCtgFilter(XElement xcf) {
            var ctg = xcf.GetAttributeValue(XNames.xnCategory);
            var sctgs = xcf.GetAttributeValue(XNames.xaValue);
            if (string.IsNullOrEmpty(sctgs)) return null;
            CtgFilter rz = new CtgFilter() {
                UseNegate = xcf.GetAttributeBool(XNames.xaNegate),
                UseAndRule = xcf.GetAttributeBool(XN.xaUseAnd),
                Categories = new HashSet<string>(sctgs.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.InvariantCultureIgnoreCase),
                Definition = Project.CategoryDefinitions.FirstOrDefault(r => ctg.Equals(r.Title, StringComparison.InvariantCultureIgnoreCase))
            };
            
            if (rz.Definition==null) {
                return null;
            }
            return rz;
        }

        public static XElement GetDefaultXmlCfg() {
            var xrz = new XElement(XPrime,
                new XComment(string.Format("Supported Cues: {0}\r\n"+
                "Optional attributes: {1}, {2}\r\n"
                , string.Join("|", Enum.GetNames(typeof(UiCues))),
                XN.xaUseAnd, XNames.xaNegate
                )));
            var xtx = new XElement(XNames.xnItem,
                new XAttribute(XNames.xaOrderW, 1),
                new XAttribute(XN.xaCue, UiCues.Style3),
                new XAttribute(XNames.xaType, "text"),
                new XAttribute(XN.xaField, cnFldNames),
                new XAttribute(XN.xaFilter, "lens: --"),
                new XComment(string.Format("If {0} starts from '{1}' then Regex match is used", XN.xaFilter, cnRegex))
                );
            xrz.Add(xtx);
            xrz.Add(new XElement(XNames.xnItem,
                new XAttribute(XNames.xaOrderW, 2),
                new XAttribute(XN.xaCue, UiCues.Trash),
                new XAttribute(XNames.xaType, "ctg"),
                new XAttribute(XNames.xnCategory, "Publish to"),
                new XAttribute(XNames.xaValue, "Trash|Vertical"),
                new XComment(string.Format("Selects by category"))
                ));
            return xrz;
        }
    }

    public class UiCueRule  {
        public int Order { get; set; }
        public UiCues Cue { get; set; }
        public IMediaFileFilter Filter { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Ops.NetCoe.LightFrame;

namespace KmlOrg {
    class KmlConverter {
        public Dictionary<string, string> StyleIdMap { get; protected set; }
        public string DefaultStyleId { get; set; }
        public bool SwapLonLatWhenParsing { get; set; }
        //public List<CodeValuePair> Keyword2Style { get; protected set; }
        public List<KmlMarkerRule> KmlStyleSelectors { get; protected set; }

        public KmlConverter() {
            this.StyleIdMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            this.DefaultStyleId = Constants.StyleIds.Red;
            foreach(var r in Constants.StyleIds.GetItems()) {
                this.StyleIdMap[r] = r;
            }
            KmlStyleSelectors = new List<KmlMarkerRule>();
            //Keyword2Style = new List<CodeValuePair>();
        }

        /// <summary>
        /// Enumerate non-empty lines
        /// </summary>
        /// <param name="src">Source text. Can be <c>null</c>.</param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateLines(string src) {
            if (string.IsNullOrEmpty(src))
                return new string[0];
            string[] lines = src.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return lines;
        }

        /// <summary>
        /// Expected line "Name|Coordinates[|Type[|Description]]"
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public List<KmlPlacemark> ParsePlacemarks(string src) {
            var rz = new List<KmlPlacemark>();
            if (string.IsNullOrEmpty(src)) return rz;
            KmlPlacemark pm;
            string[] parts;
            foreach(var ln in EnumerateLines(src)) {
                parts = ln.Split('|');
                if (parts.Length == 0) continue;
                pm = new KmlPlacemark() {
                    Title = parts[0].Trim()
                };
                if (parts.Length>1) {
                    pm.Coordinates = parts[1].Trim();
                    if (this.SwapLonLatWhenParsing) {
                        pm.Coordinates = SwapLonLat(pm.Coordinates);
                    }
                }
                else {
                    pm.Coordinates = "0,0";
                }
                if (parts.Length>2) {
                    pm.Marker = parts[2].Trim();
                    pm.StyleUrl = GetStyleUrl(pm.Marker);
                }
                else {
                    pm.StyleUrl = GetStyleUrlByTitle(pm.Title);
                }
                if (parts.Length>3) {
                    pm.Description = parts[3].Trim();
                }
                rz.Add(pm);
            }

            return rz;
        }

        static string SwapLonLat(string coord) {
            string[] parts = coord.Split(',');
            if (parts.Length>=2) {
                var t = parts[1];
                parts[1] = parts[0];
                parts[0] = t;
                return string.Join(", ", parts);
            }
            return coord;
        }

        string GetStyleUrl(string key) {
            string val;
            if (this.StyleIdMap.TryGetValue(key, out val))
                return val;
            return DefaultStyleId;
        }

        /// <summary>
        /// Get style using keywords from title
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        string GetStyleUrlByTitle(string title) {
            foreach (var ss in this.KmlStyleSelectors) {
                if (ss.IsMatch(title))
                    return GetStyleUrl(ss.KmlMarker);
            }
            return DefaultStyleId;
        }


        ///// <summary>
        ///// Get style using keywords from title
        ///// </summary>
        ///// <param name="title"></param>
        ///// <returns></returns>
        //string GetStyleUrlByTitleOld(string title) {
        //    foreach(var kws in this.Keyword2Style) {
        //        if (title.IndexOf(kws.Code, StringComparison.InvariantCultureIgnoreCase)>=0) {
        //            return GetStyleUrl(kws.Value);
        //        }
        //    }
        //    return DefaultStyleId;
        //}

        public XElement GetStyleXml(string key) {
            string id = key.Substring(1);
            XElement xrz = new XElement(XKM.nsKml + "Style", new XAttribute("id", id),
                new XElement(XKM.nsKml + "IconStyle",
                  new XElement(XKM.nsKml + "Icon", new XElement(XKM.nsKml + "href", string.Format("http://mapswith.me/placemarks/{0}.png", id)) )
                )
            );
            return xrz;
        }

        public XElement ToKml(IEnumerable<KmlPlacemark> placemarks, string name= "My Places") {
            //XDocument xd = new XDocument(new XDeclaration("1.0", "UTF-8", ""));
            //return xd;
            XElement xdc, xrz = new XElement(XKM.nsKml + "kml", new XAttribute("xmlns", "http://earth.google.com/kml/2.2"));
            xrz.Add(xdc = new XElement(XKM.nsKml+ "Document"));
            foreach(var r in Constants.StyleIds.GetItems()) {
                xdc.Add(GetStyleXml(r));
            }
            xdc.Add(new XElement(XKM.nsKml + "name", name));
            xdc.Add(new XElement(XKM.nsKml + "visibility", 1));
            // Maps me shows places in reverse order as they are in KML
            foreach(var r in (from p in placemarks orderby p.Title.ToLower() descending select p)) {
                if (r.IsValid)
                    xdc.Add(r.GetXml());
            }
            return xrz;
        }

        public void ApplyXml(string xmlCfg) {
            XElement xel = XElement.Parse(xmlCfg);
            var xsm = xel.Element("kmlStyleMap");
            List<KmlMarkerRule> rules = new List<KmlMarkerRule>();
            if (xsm!=null) {
                this.DefaultStyleId = xsm.GetAttributeValue("default", Constants.StyleIds.Red);
                string tmp;
                int ix = 1;
                KmlMarkerRule kmr;
                foreach (var xit in xsm.Elements("item")) {
                    kmr = new KmlMarkerRule() { Order = ix++ };
                    if (kmr.ApplyXml(xit)) {
                        rules.Add(kmr);
                    }
                    else if (!string.IsNullOrEmpty(kmr.KmlMarker) && (null != (tmp = xit.GetAttributeValue("marker", null)))) {
                        this.StyleIdMap[tmp.Trim()] = kmr.KmlMarker;
                    }
                }
                this.KmlStyleSelectors = new List<KmlMarkerRule>(from r in rules orderby r.Order select r);
            }
        }

        //public void ApplyXmlOld(string xmlCfg) {
        //    XElement xel = XElement.Parse(xmlCfg);
        //    var xsm = xel.Element("kmlStyleMap");
        //    char[] seps = new char[] { ';', '|' };
        //    if (xsm != null) {
        //        this.DefaultStyleId = xsm.GetAttributeValue("default", Constants.StyleIds.Red);
        //        string styleId, tmp;
        //        string[] keywords;
        //        foreach (var xit in xsm.Elements("item")) {
        //            if (null == (styleId = xit.GetAttributeValue("value", null))) continue;
        //            styleId = styleId.Trim();
        //            if (null != (tmp = xit.GetAttributeValue("keywords", null))) {
        //                keywords = tmp.Split(seps, StringSplitOptions.RemoveEmptyEntries);
        //                foreach (var kw in keywords) {
        //                    this.Keyword2Style.Add(new CodeValuePair(kw.Trim(), styleId));
        //                }
        //            }
        //            else if (null != (tmp = xit.GetAttributeValue("marker", null))) {
        //                this.StyleIdMap[tmp.Trim()] = styleId;
        //            }
        //        }
        //    }
        //}


        public class KmlMarkerRule {
            static char[] seps = new char[] { ';', '|' };
            public int Order { get; set; }
            public string KmlMarker { get; set; }
            public Regex RgxKw { get; set; }
            public Regex Rgx { get; set; }

            public bool ApplyXml(XElement xmlCfg) {
                HashSet<string> keywords;
                this.KmlMarker = xmlCfg.GetAttributeValue("value", null);
                if (string.IsNullOrEmpty(this.KmlMarker))
                    return false;
                var ord = xmlCfg.GetAttributeInt("order");
                if (ord.HasValue)
                    this.Order = ord.Value;
                var stmp = xmlCfg.GetAttributeValue("keywords");
                if (!string.IsNullOrEmpty(stmp)) {
                    keywords = new HashSet<string>(stmp.Split(seps, StringSplitOptions.RemoveEmptyEntries), StringComparer.CurrentCultureIgnoreCase);
                    stmp = string.Join("|", keywords);
                    try {
                        RgxKw = new Regex(stmp, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    }
                    catch (Exception x) {
                        x.Data["regex.kw"] = stmp;
                    }
                }
                stmp = xmlCfg.GetAttributeValue("regex");
                if (!string.IsNullOrEmpty(stmp)) {
                    try {
                        Rgx = new Regex(stmp, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    }
                    catch (Exception x) {
                        x.Data["xmlNode"] = xmlCfg.ToString().GetLeft(80);
                        x.Data["regex"] = stmp;
                        throw;
                    }
                }
                return (RgxKw != null)||(Rgx!=null);
            }

            public bool IsMatch(string toCheck) {
                if (string.IsNullOrEmpty(toCheck)) return false;
                if ((Rgx != null) && Rgx.IsMatch(toCheck)) return true;
                if (RgxKw != null && RgxKw.IsMatch(toCheck)) return true;
                return false;
            }
        }
    }
}

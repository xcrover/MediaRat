using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Ops.NetCoe.LightFrame;

namespace KmlOrg {

    /// <summary>
    /// GPX files converter
    /// <see cref="http://www.topografix.com/GPX/1/1/"/>
    /// Dev guide <see cref="http://www.topografix.com/gpx_manual.asp#gpx_req"/>
    /// </summary>
    public class GpxConverter {

        /// <summary>
        /// Extracts Routes (rte) and transforms to Track (trk) - this is just to allow showing track name in Wikiloc.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public XElement RteToTrk(XElement src, string name=null, string comment=null, string description=null) {
            var creator = src.GetAttributeValue(XN.xaCreator);
            creator = string.IsNullOrEmpty(creator) ? "XC KmlOrg.rte" : ("XC converted from "+creator);
            var xrz = CreateGpx(creator);
            var xel = src.Element(XN.xnMetadata);
            if (xel!=null) 
                xrz.Add(new XElement(xel));
            var xrtes = src.Elements(XN.xnRte);
            var xtrk = new XElement(XN.xnTrk);
            xrz.Add(xtrk);

            xtrk.Add(new XElement(XN.xnName, new XCData(GetNonEmpty(name))));
            xtrk.Add(new XElement(XN.xnComment, new XCData(GetNonEmpty(comment, name))));
            xtrk.Add(new XElement(XN.xnDescription, new XCData(GetNonEmpty(description, comment, name))));

            xtrk.AddElementIf(XN.xnNumber, "1");
            XElement xtseg, xpt;
            foreach (var xrte in xrtes) {
                xtseg = new XElement(XN.xnTrkSeg);
                xtrk.Add(xtseg);
                foreach(var xrp in xrte.Elements(XN.xnRtePt)) {
                    xpt = new XElement(XN.xnTrkPt);
                    xpt.Add(from xa in xrp.Attributes() select new XAttribute(xa));
                    xpt.Add(from xe in xrp.Elements() select new XElement(xe));
                    xtseg.Add(xpt);
                }
            }
            return xrz;
        }

        /// <summary>
        /// Extracts Way points (wpt) and transforms to Track (trk) - this is just to allow showing track name in Wikiloc.
        /// </summary>
        /// <param name="src">Source GPX</param>
        /// <returns></returns>
        public XElement WptToTrk(XElement src, string name = null, string comment = null, string description = null) {
            var creator = src.GetAttributeValue(XN.xaCreator);
            creator = string.IsNullOrEmpty(creator) ? "XC KmlOrg.wpt" : ("XC converted from " + creator);
            var xrz = CreateGpx(creator);
            var xel = src.Element(XN.xnMetadata);
            if (xel != null)
                xrz.Add(new XElement(xel));
            var xtrk = new XElement(XN.xnTrk);
            xrz.Add(xtrk);

            xtrk.Add(new XElement(XN.xnName, new XCData(GetNonEmpty(name))));
            xtrk.Add(new XElement(XN.xnComment, new XCData(GetNonEmpty(comment, name))));
            xtrk.Add(new XElement(XN.xnDescription, new XCData(GetNonEmpty(description, comment, name))));
            xtrk.AddElementIf(XN.xnNumber, "1");
            XElement xtseg, xpt;
            xtseg = new XElement(XN.xnTrkSeg);
            xtrk.Add(xtseg);
            foreach (var xrp in src.Elements(XN.xnWpt)) {
                xpt = new XElement(XN.xnTrkPt);
                xpt.Add(from xa in xrp.Attributes() select new XAttribute(xa));
                xpt.Add(from xe in xrp.Elements() select new XElement(xe));
                xtseg.Add(xpt);
            }
            return xrz;
        }

        /// <summary>
        /// Copy way points (wpt) from <paramref name="srcGpx"/> to <paramref name="trgGpx"/>
        /// </summary>
        /// <param name="srcGpx"></param>
        /// <param name="trgGpx"></param>
        void CopyWpt(XElement srcGpx, XElement trgGpx) {
            foreach (var xrp in srcGpx.Elements(XN.xnWpt)) {
                trgGpx.Add(new XElement(xrp));
            }
        }

        /// <summary>
        /// Create empty GPX document
        /// </summary>
        /// <param name="creator"></param>
        /// <returns></returns>
        public XElement CreateGpx(string creator) {
            var xrz = new XElement(XN.nsGpx.GetName("gpx"),
                new XAttribute("xmlns", XN.nsGpx.NamespaceName),
                new XAttribute("version", "1.1"),
                new XAttribute(XN.xaCreator, creator??"XC KmlOrg"),
                new XAttribute(XNamespace.Xmlns+ "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                new XAttribute(XN.nsXsi+ "schemaLocation", "http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd")
                );
            return xrz;
        }

        static string GetNonEmpty(params string[] args) {
            foreach(var s in args) {
                if (!string.IsNullOrEmpty(s))
                    return s;
            }
            return "-?!-";
        }

        public class XN : XKM {
            public static XName xaCreator = XName.Get("creator");
            public static XName xnMetadata = XKM.nsGpx.GetName("metadata");
            public static XName xnRte = XKM.nsGpx.GetName("rte");
            public static XName xnName = XKM.nsGpx.GetName("name");
            public static XName xnComment = XKM.nsGpx.GetName("cmt");
            public static XName xnDescription = XKM.nsGpx.GetName("desc");
            public static XName xnNumber = XKM.nsGpx.GetName("number");
            public static XName xnRtePt = XKM.nsGpx.GetName("rtept");
            public static XName xnTrk = XKM.nsGpx.GetName("trk");
            public static XName xnTrkSeg = XKM.nsGpx.GetName("trkseg");
            public static XName xnTrkPt = XKM.nsGpx.GetName("trkpt");
            public static XName xnExtensions = XKM.nsGpx.GetName("extensions");
            public static XName xnColor = XKM.nsGpx.GetName("color");
            public static XName xnWpt = XKM.nsGpx.GetName("wpt");
        }
    }
}

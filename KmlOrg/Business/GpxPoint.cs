using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KmlOrg {
    /// <summary>
    /// GPX Way point
    /// </summary>
    public class GpxWPt {
        public static XName XPrime { get { return XN.xnWpt; } }
        public double Lon { get; set; }
        public double Lat { get; set; }
        public double? Ele { get; set; }
        public DateTime? Time { get; set; }
        

        protected class XN : XKM {
            public static XName xnName = XKM.nsGpx.GetName("name");
            public static XName xnComment = XKM.nsGpx.GetName("cmt");
            public static XName xnDescription = XKM.nsGpx.GetName("desc");
            public static XName xnNumber = XKM.nsGpx.GetName("number");
            public static XName xnRtePt = XKM.nsGpx.GetName("rtept");
            public static XName xnTrkPt = XKM.nsGpx.GetName("trkpt");
            public static XName xnExtensions = XKM.nsGpx.GetName("extensions");
            public static XName xnWpt = XKM.nsGpx.GetName("wpt");
            public static XName xaLon = XKM.nsGpx.GetName("lon");
            public static XName xaLat = XKM.nsGpx.GetName("lat");
            public static XName xnEle = XKM.nsGpx.GetName("ele");
            public static XName xnTime = XKM.nsGpx.GetName("time");
        }

        public virtual XName XNm => XPrime;

        public virtual XElement ToXml(XName xnm) {
            return ToXml(new XElement(xnm));
        }

        public virtual XElement ToXml(XElement xtrg=null) {
            if (xtrg == null)
                xtrg = new XElement(XPrime);
            xtrg.Add(new XAttribute(XN.xaLon, this.Lon));
            xtrg.Add(new XAttribute(XN.xaLat, this.Lat));
            if (Ele.HasValue)
                xtrg.Add(new XElement(XN.xnEle, this.Ele.Value));
            if (this.Time.HasValue)
                xtrg.Add(new XElement(XN.xnTime, this.Time.Value));
            return xtrg;
        }

    }

    public class GpxRtePt : GpxWPt {
        new public static XName XPrime { get { return XN.xnRtePt; } }
        public override XName XNm => XPrime;
    }

    public class GpxTrkPt : GpxWPt {
        new public static XName XPrime { get { return XN.xnTrkPt; } }
        public override XName XNm => XPrime;
    }

}

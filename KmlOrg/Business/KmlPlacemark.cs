using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Ops.NetCoe.LightFrame;

namespace KmlOrg {
    public class KmlPlacemark : NotifyPropertyChangedBase, IXmlSource, IXmlConfigurable {
        public static XName XPrime {  get { return XN.xnPlacemark; } }
        ///<summary>Title</summary>
        private string _title;
        ///<summary>Description</summary>
        private string _description;
        ///<summary>Coordinates</summary>
        private string _coordinates;
        ///<summary>Marker</summary>
        private string _marker;
        ///<summary>Style URL</summary>
        private string _styleUrl;

        ///<summary>Style URL</summary>
        public string StyleUrl {
            get { return this._styleUrl; }
            set {
                if (this._styleUrl != value) {
                    this._styleUrl = value;
                    this.FirePropertyChanged(nameof(StyleUrl));
                }
            }
        }


        ///<summary>Marker</summary>
        public string Marker {
            get { return this._marker; }
            set {
                if (this._marker != value) {
                    this._marker = value;
                    this.FirePropertyChanged(nameof(Marker));
                }
            }
        }


        ///<summary>Coordinates</summary>
        public string Coordinates {
            get { return this._coordinates; }
            set {
                if (this._coordinates != value) {
                    this._coordinates = value;
                    this.FirePropertyChanged(nameof(Coordinates));
                }
            }
        }


        ///<summary>Description</summary>
        public string Description {
            get { return this._description; }
            set {
                if (this._description != value) {
                    this._description = value;
                    this.FirePropertyChanged(nameof(Description));
                }
            }
        }


        ///<summary>Title</summary>
        public string Title {
            get { return this._title; }
            set {
                if (this._title != value) {
                    this._title = value;
                    this.FirePropertyChanged(nameof(Title));
                }
            }
        }

        /// <summary>
        /// Checks if this definition is valid
        /// </summary>
        public bool IsValid {
            get {
                return !string.IsNullOrWhiteSpace(this.Title) &&
                    !string.IsNullOrWhiteSpace(this.Coordinates);
            }
        }

        #region XML Conversion

        public class XN : XKM {
            ///<summary>The XML name for Placemark [Placemark]</summary>
            public static XName xnPlacemark = nsKml.GetName("Placemark");
            ///<summary>The XML name for Name [name]</summary>
            public static XName xnName = nsKml.GetName("name");
            ///<summary>The XML name for StyleUrl [styleUrl]</summary>
            public static XName xnStyleUrl = nsKml.GetName("styleUrl");
            ///<summary>The XML name for Point [Point]</summary>
            public static XName xnPoint = nsKml.GetName("Point");
            ///<summary>The XML name for Coordinates [coordinates]</summary>
            public static XName xnCoordinates = nsKml.GetName("coordinates");
            ///<summary>The XML name for Description [description]</summary>
            public static XName xnDescription = nsKml.GetName("description");

        }

        public void ApplyConfiguration(XElement configSource, string password = null) {
            configSource.TrGetElementVal(XN.xnName, v => this.Title = v);
            configSource.TrGetElementVal(XN.xnDescription, v => this.Description = v);
            configSource.TrGetElementVal(XN.xnStyleUrl, v => this.StyleUrl = v);
            var xp = configSource.Element(XN.xnPoint);
            if (xp!=null) {
                xp.TrGetElementVal(XN.xnCoordinates, v => this.Coordinates = v);
            }
        }

        public XElement GetXml(string password = null) {
            XElement xrz = new XElement(XPrime);
            xrz.AddElementIf(XN.xnName, this.Title, "-?!-");
            xrz.AddElementIf(XN.xnDescription, this.Description);
            xrz.AddElementIf(XN.xnStyleUrl, this.StyleUrl, Constants.StyleIds.Red);
            var xp = new XElement(XN.xnPoint);
            xrz.Add(xp);
            xp.AddElementIf(XN.xnCoordinates, this.Coordinates, "0,0");
            return xrz;
        }

        #endregion
    }
}

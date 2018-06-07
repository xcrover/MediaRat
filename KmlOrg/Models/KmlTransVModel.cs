using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Ops.NetCoe.LightFrame;

namespace KmlOrg.Models {
    public class KmlTransVModel : WorkspaceViewModel {
        const string cnDefaultKmlTitle = "MyPlaces";
        ///<summary>Formatted text</summary>
        private string _text;
        ///<summary>KML Placemarks</summary>
        private ObservableCollection<KmlPlacemark> _placemarks= new ObservableCollection<KmlPlacemark>();
        ///<summary>KML Name</summary>
        private string _kmlTitle;
        ///<summary>KML Text</summary>
        private string _kmlText;
        ///<summary>Switch longitude and latitude in coordinates to adress mapsMe error</summary>
        private bool _switchLonLat;
        ///<summary>XML Settings</summary>
        private string _xmlSettings;
        ///<summary>Description</summary>
        private string _description;
        ///<summary>GPX result</summary>
        private string _gpxResult;



        #region Properties
        ///<summary>Switch longitude and latitude in coordinates to adress mapsMe error</summary>
        public bool SwitchLonLat {
            get { return this._switchLonLat; }
            set {
                if (this._switchLonLat != value) {
                    this._switchLonLat = value;
                    this.FirePropertyChanged(nameof(SwitchLonLat));
                }
            }
        }


        ///<summary>KML Text</summary>
        ///<summary>XML Settings</summary>
        public string XmlSettings {
            get { return this._xmlSettings; }
            set {
                if (this._xmlSettings != value) {
                    this._xmlSettings = value;
                    this.FirePropertyChanged(nameof(XmlSettings));
                }
            }
        }


        public string KmlText {
            get { return this._kmlText; }
            set {
                if (this._kmlText != value) {
                    this._kmlText = value;
                    this.FirePropertyChanged(nameof(KmlText));
                }
            }
        }

        ///<summary>KML Placemarks</summary>
        public ObservableCollection<KmlPlacemark> Placemarks {
            get { return this._placemarks; }
            set {
                if (this._placemarks != value) {
                    this._placemarks = value;
                    this.FirePropertyChanged(nameof(Placemarks));
                }
            }
        }

        ///<summary>Formatted text</summary>
        public string Text {
            get { return this._text; }
            set {
                if (this._text != value) {
                    this._text = value;
                    this.FirePropertyChanged(nameof(Text));
                }
            }
        }

        ///<summary>KML Name</summary>
        public string KmlTitle {
            get { return this._kmlTitle; }
            set {
                if (this._kmlTitle != value) {
                    this._kmlTitle = value;
                    this.FirePropertyChanged(nameof(KmlTitle));
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

        ///<summary>GPX result</summary>
        public string GpxResult {
            get { return this._gpxResult; }
            set {
                if (this._gpxResult != value) {
                    this._gpxResult = value;
                    this.FirePropertyChanged(nameof(GpxResult));
                }
            }
        }

        #endregion

        #region Construction

        public KmlTransVModel() {
            this.Status = new StatusVModel();
            this.InitCommands();
            this.KmlTitle = cnDefaultKmlTitle;
            LoadKmlConfig();
        }

        void LoadKmlConfig() {
            string fileNm = this.GetType().Assembly.CodeBase.Replace("file:///", "");
            fileNm = Path.GetDirectoryName(fileNm);
            fileNm = Path.Combine(fileNm, "KmlConfig.xml");
            try {
                this.XmlSettings= File.ReadAllText(fileNm);
            }
            catch (Exception x) {
                x.Data["file"] = fileNm;
                this.ReportError(x);
                this.XmlSettings = cnXmlSettings;
            }
        }

const string cnXmlSettings= @"<?xml version = ""1.0"" encoding=""utf-8"" ?> 
<configuration>
  <kmlStyleMap default=""#placemark-red"">
    <item value = ""#placemark-blue"" keywords=""restaurant;buffet;cafe;eatery;dining;"" />
    <item value = ""#placemark-orange"" keywords=""hotel;lodge;motel;"" />
  </kmlStyleMap>
</configuration>";

        #endregion

        #region Commands

        public ICommand CmdParseText { get; protected set; }
        public ICommand CmdParseKml { get; protected set; }
        public ICommand CmdSelectKml { get; protected set; }
        public ICommand CmdSaveAs { get; protected set; }
        public ICommand CmdGpxRte2Trk { get; protected set; }
        public ICommand CmdGpxWpt2Trk { get; protected set; }
        public ICommand CmdSaveGpx { get; protected set; }

        public void InitCommands() {
            this.CmdSelectKml = new DelegateCommand(DoSelectKlm);
            this.CmdParseText = new DelegateCommand(DoParseText, () => !string.IsNullOrEmpty(this.Text));
            this.CmdParseKml = new DelegateCommand(DoParseKml, () => !string.IsNullOrEmpty(this.KmlText));
            this.CmdSaveAs = new DelegateCommand(DoSaveAs, () => this.Placemarks.Count>0);
            this.CmdGpxRte2Trk = new DelegateCommand(DoGpxRte2Trk, () => !string.IsNullOrEmpty(this.Text));
            this.CmdGpxWpt2Trk = new DelegateCommand(DoGpxWpt2Trk, () => !string.IsNullOrEmpty(this.Text));
            this.CmdSaveGpx = new DelegateCommand(DoSaveGpx, () => !string.IsNullOrEmpty(this.GpxResult));
            //this.CmdStartGroup = new DelegateCommand(DoStartGroup, () => this.CurrentFileDsc != null);
            //this.CmdAddToPreviousGroup = new DelegateCommand(DoAddToPreviousGroup, () => this.CurrentGroup != null);
            //this.CmdAddToNextGroup = new DelegateCommand(DoAddToNextGroup, () => this.CurrentGroup != null);
        }

        void DoSelectKlm() {
            FileAccessRequest far = new FileAccessRequest() {
                IsForReading = true,
                IsMultiSelect = false,
                ExtensionFilter = "KML Documents (*.kml)|*.kml|All files (*.*)|*.*",
                ExtensionFilterIndex = 1,
                SuggestedFileName = cnDefaultKmlTitle + ".kml"
            };
            string fileNm = null;
            if (AppContext.Current.GetServiceViaLocator<IUIHelper>().TrySelectFiles(far, f => fileNm = f)) {

            }
        }



        void DoParseText() {
            try {
                KmlConverter kcnv = new KmlConverter();
                kcnv.ApplyXml(this.XmlSettings);
                kcnv.SwapLonLatWhenParsing = this.SwitchLonLat;

                var items = kcnv.ParsePlacemarks(this.Text);
                this.Placemarks.Clear();
                foreach (var r in items) {
                    this.Placemarks.Add(r);
                }
            }
            catch (Exception x) {
                this.ReportError(x);
            }
        }

        void DoParseKml() {
            this.Status.SetError("KML Parsing not implemented yet");
        }


        void DoSaveAs() {
            FileAccessRequest far = new FileAccessRequest() {
                IsForReading = false,
                IsMultiSelect = false,
                ExtensionFilter = "KML Documents (*.kml)|*.kml|All files (*.*)|*.*",
                ExtensionFilterIndex = 1,
                SuggestedFileName = cnDefaultKmlTitle+".kml"
            };
            string fileNm=null;
            if (AppContext.Current.GetServiceViaLocator<IUIHelper>().TrySelectFiles(far, f=>fileNm= f)) {
                string fnTitle = Path.GetFileNameWithoutExtension(fileNm);
                if (!cnDefaultKmlTitle.Equals(fnTitle, StringComparison.InvariantCultureIgnoreCase) && cnDefaultKmlTitle.Equals(this.KmlTitle))
                    this.KmlTitle = fnTitle.Trim();
                KmlConverter kcnv = new KmlConverter();
                var xkml= kcnv.ToKml(this.Placemarks, this.KmlTitle);
                XDocument xdoc = new XDocument(new XDeclaration("1.0", "UTF-8", null), xkml);
                this.KmlText = xdoc.ToString();
                xdoc.Save(fileNm);
            }
        }

        #region GPX
        void DoGpxRte2Trk() {
            try {
                var xsrc = XDocument.Parse(this.Text);
                if (xsrc.Root?.Name.LocalName != "gpx")
                    throw new BizException("GPX document expected");
                var cnv = new GpxConverter();
                var xrz= cnv.RteToTrk(xsrc.Root, this.KmlTitle, this.Description);
                var xdc = xrz.EnsureXDoc();
                this.GpxResult = xdc.ToStringWithDeclaration();
            }
            catch (Exception x) {
                this.ReportError(x);
            }
        }

        void DoGpxWpt2Trk() {
            try {
                var xsrc = XDocument.Parse(this.Text);
                if (xsrc.Root?.Name.LocalName != "gpx")
                    throw new BizException("GPX document expected");
                var cnv = new GpxConverter();
                var xrz = cnv.WptToTrk(xsrc.Root, this.KmlTitle, this.Description);
                var xdc = xrz.EnsureXDoc();
                this.GpxResult = xdc.ToStringWithDeclaration();
            }
            catch (Exception x) {
                this.ReportError(x);
            }
        }


        void DoSaveGpx() {
            FileAccessRequest far = new FileAccessRequest() {
                IsForReading = false,
                IsMultiSelect = false,
                ExtensionFilter = "GPX Documents (*.gpx)|*.gpx|All files (*.*)|*.*",
                ExtensionFilterIndex = 1,
                SuggestedFileName = cnDefaultKmlTitle + ".gpx"
            };
            string fileNm = null;
            if (AppContext.Current.GetServiceViaLocator<IUIHelper>().TrySelectFiles(far, f => fileNm = f)) {
                try {
                    using (var fo = System.IO.File.CreateText(fileNm)) {
                        fo.Write(this.GpxResult);
                        fo.Close();
                    }
                }
                catch (Exception x) {
                    x.Data["file"] = fileNm;
                    this.ReportError(x);
                }
            }
        }

        #endregion

        #endregion

        public override ICommand FindCommand(Func<ICommand, bool> filter) {
            throw new NotImplementedException();
        }
    }
}

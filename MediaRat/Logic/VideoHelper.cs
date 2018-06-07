using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
    public class VideoHelper {
        /// <summary>
        /// The cn media format
        /// </summary>
        public const string cnMediaFormat = "Format";
        /// <summary>
        /// The cn duration
        /// </summary>
        public const string cnDuration = "Duration";
        /// <summary>
        /// The cn bitrate
        /// </summary>
        public const string cnBitrate = "Bitrate";
        /// <summary>
        /// The cn Width
        /// </summary>
        public const string cnWidth = "Width";
        /// <summary>
        /// The cn Height
        /// </summary>
        public const string cnHeight = "Height";


        ///<summary>ffProbe path</summary>
        private string _ffProbePath;

        ///<summary>ffProbe path</summary>
        public string FfProbePath {
            get { return this._ffProbePath ?? (this._ffProbePath = GetFfProbePath()); }
            set { this._ffProbePath = value; }
        }
        

        public KeyValuePairXCol<string, string> GetMetadataShell(MediaFile source) {
            KeyValuePairXCol<string, string> rz = new KeyValuePairXCol<string, string>();
            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder objFolder;
            var srcFolder= Path.GetDirectoryName(source.FullName);
            string val;
            objFolder = shell.NameSpace(srcFolder);
            List<string> arrHeaders = new List<string>();
            for (int i = 0; i < short.MaxValue; i++) {
                string header = objFolder.GetDetailsOf(null, i);
                if (String.IsNullOrEmpty(header))
                    break;
                arrHeaders.Add(header);
            }
            
            foreach (Shell32.FolderItem2 item in objFolder.Items()) {
                if (string.Compare(item.Name, source.Title, true) == 0) {
                    rz.AddRange(GetXProp(item, "Bitrate", "Frame rate", "Duration"));
                    for (int i = 0; i < arrHeaders.Count; i++) {
                        val = objFolder.GetDetailsOf(item, i);
                        if (!string.IsNullOrWhiteSpace(val)) {
                            rz.Add(arrHeaders[i], val);
                        }
                        //System.Diagnostics.Debug.WriteLine("{0}\t{1}: {2}", i, arrHeaders[i], objFolder.GetDetailsOf(item, i));
                    }
                    break;
                }
            }
            return rz;
        }

        IEnumerable<KeyValuePairX<string, string>> GetXProp(Shell32.FolderItem2 src, params string[] xPropNames) {
            foreach (var xpn in xPropNames) {
                var tmp = src.ExtendedProperty(xpn);
                if (tmp != null) {
                    var rz = tmp.ToString();
                    if (!string.IsNullOrWhiteSpace(rz))
                        yield return new KeyValuePairX<string, string>() { Key = xpn, Value = rz };
                }
            }
        }

        string GetFfProbePath() {
            const string ffProbeKey= "ffProbe.Path";
            string rz=AppContext.Current.GetAppCfgItem(ffProbeKey);
            if (rz == null)
                throw new BizException(string.Format("Invalid configuration: appSetings['{0}'] must point to the ffProbe.exe", ffProbeKey));
            return rz;
        }

        /// <summary>
        /// Gets the ff probe XML.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <returns></returns>
        /// <exception cref="Ops.NetCoe.LightFrame.BizException"></exception>
        public XElement GetFFProbeXml(string sourcePath) {
            StringBuilder sout = new StringBuilder(1024), serr = new StringBuilder(512);
            string args = string.Format("-i \"{0}\" -print_format xml -sexagesimal -show_error -show_format -show_streams", sourcePath);
            string action = null;
            string xtmp = null;
            try {
                action = string.Format("execute and parse results: \"{0}\" {1}", this.FfProbePath, args);
                OSCommand.RunAndWait(this.FfProbePath, args, (s) => sout.AppendLine(s), (s) => serr.AppendLine(s));
                System.Threading.Thread.Sleep(20); // Sometimes sout is not populated to the end, need some cycles
                XElement xff = XElement.Parse(xtmp = sout.ToString());
                return xff;
            }
            catch (Exception x) {
                AppContext.Current.LogTechError(string.Format("Failed to {0}", action), x);
                System.Diagnostics.Debug.WriteLine(xtmp);
                throw new BizException(x.ToShortMsg(action));
            }
        }

        public bool TryGetMetadata(string sourceFile, out VideoStreamCfg videoCfg, out AudioStreamCfg audioCfg) {
            videoCfg = null;
            audioCfg = null;
            try {
                var xff = GetFFProbeXml(sourceFile);
                videoCfg = new VideoStreamCfg();
                XElement xe = xff.Element("format");
                int? tInt;
                int tmp=0;
                string tStr;
                TimeSpan tTs= TimeSpan.Zero;
                ulong sz=0;
                if (xe != null) {
                    videoCfg.MediaFormat = xe.GetAttributeValue("format_name");
                    tInt= xe.GetAttributeInt("bit_rate");
                    videoCfg.Bitrate= tInt.HasValue ? tInt.Value : 0;
                    if (xe.TryGetAttribute<ulong>("size", (x) => { sz = (ulong)x; return true; }))
                        videoCfg.Size = sz;
                    if (xe.TryGetAttribute<ulong>("duration", (x) => TimeSpan.TryParse(x.Value, out tTs)))
                        videoCfg.Duration = tTs;
                }
                string codecType = null; 
                foreach (var xs in xff.XPathSelectElements("streams/stream")) {
                    codecType = xs.GetAttributeValue("codec_type");
                    switch (codecType) {
                        case "video": {
                                videoCfg.Codec = xs.GetAttributeValue("codec_name");
                                if (xs.TryGetAttribute<int>("height", (x) => int.TryParse(x.Value, out tmp)))
                                    videoCfg.Height = tmp;
                                if (xs.TryGetAttribute<int>("width", (x) => int.TryParse(x.Value, out tmp)))
                                    videoCfg.Width = tmp;
                                videoCfg.AspectRatio = xs.GetAttributeValue("display_aspect_ratio");
                                videoCfg.PixelFormat = xs.GetAttributeValue("pix_fmt");
                                videoCfg.FrameRate = xs.GetAttributeValue("r_frame_rate");
                            }
                            break;

                        case "audio": {
                                audioCfg = new AudioStreamCfg();
                                audioCfg.Codec = xs.GetAttributeValue("codec_name");
                                if (xs.TryGetAttribute<int>("bit_rate", (x) => int.TryParse(x.Value, out tmp)))
                                    audioCfg.Bitrate = tmp;
                                audioCfg.Channels = xs.GetAttributeValue("channels");
                            }
                            break;
                    }
                }

                return true;
            }
            catch (BizException bx) {
                return false;
            }
            catch (Exception x) {
                AppContext.Current.LogTechError(string.Format("Failed to get metadata for \"{0}\"", sourceFile), x);
                return false;
            }
        }

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <returns></returns>
        /// <exception cref="Ops.NetCoe.LightFrame.BizException"></exception>
        public KeyValuePairXCol<string, string> GetMetadata(string sourcePath) {
            KeyValuePairXCol<string, string> rz = new KeyValuePairXCol<string, string>();
            //StringBuilder sout = new StringBuilder(1024), serr = new StringBuilder(512);
            //string args = string.Format("-i \"{0}\" -print_format xml -sexagesimal -show_error -show_format -show_streams", sourcePath);
            string action = null;
            string xtmp = null;
            try {
                XElement xff = GetFFProbeXml(sourcePath);
                XElement xe = xff.Element("format");

                //XAttribute xa;
                if (xe != null) {
                    AddEntry(rz, xe, "format_name", cnMediaFormat);
                    AddEntry(rz, xe, "duration", cnDuration);
                    AddEntry(rz, xe, "bit_rate", cnBitrate);
                    AddEntry(rz, xe, "size", "Size");
                }
                string codecType = null; //, pfx=null;
                foreach (var xs in xff.XPathSelectElements("streams/stream")) {
                    codecType = xs.GetAttributeValue("codec_type");
                    //pfx = null;
                    switch (codecType) {
                        case "video": {
                                //pfx = "Video";
                                AddEntry(rz, xs, "codec_name", "Video Codec");
                                AddEntry(rz, xs, "height", "Height");
                                AddEntry(rz, xs, "width", "Width");
                                AddEntry(rz, xs, "display_aspect_ratio", "Aspect Ratio");
                                AddEntry(rz, xs, "pix_fmt", "Pixel Format");
                                AddEntry(rz, xs, "r_frame_rate", "Frame Rate");
                                //AddEntry(rz, xs, "duration", "Duration");
                            }
                            break;

                        case "audio": {
                                //pfx = "Audio";
                                AddEntry(rz, xs, "codec_name", "Audio Codec");
                                AddEntry(rz, xs, "bit_rate", "Audio Bit Rate");
                                AddEntry(rz, xs, "channels", "Channels");
                            }
                            break;

                        //case "subtitle": {
                        //        pfx = "Subtitle";
                        //    }
                        //    break;

                        //default:
                        //    AppContext.Current.LogTechError(string.Format("Failed to recognise stream type '{0}' in the file \"{1}\"", this.FfProbePath, args), null);
                        //    break;
                    }
                }
                action = string.Format("Get FileInfo for \"{0}\"", sourcePath);
                var fi = new FileInfo(sourcePath);
                if (fi != null) {
                    rz.Add("Size", fi.Length.ToString("##,#"));
                    rz.Add("Created", fi.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    rz.Add("Type", fi.Extension);
                }
                else {
                    rz.Add("Type", Path.GetExtension(sourcePath));
                }
            }
            catch (BizException x) {
                throw;
            }
            catch (Exception x) {
                AppContext.Current.LogTechError(string.Format("Failed to {0}", action), x);
                System.Diagnostics.Debug.WriteLine(xtmp);
                throw new BizException(x.ToShortMsg(action));
            }
            return rz;
        }


        /// <summary>
        /// Get the video file metadata.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public KeyValuePairXCol<string, string> GetMetadata(MediaFile source) {
            return GetMetadata(source.FullName);
        }

        void AddEntry(KeyValuePairXCol<string, string> trg, XElement src, XName attrName, string propName) {
            string vl = src.GetAttributeValue(attrName);
            if (!string.IsNullOrEmpty(vl)) {
                trg.Add(propName, vl);
            }
        }

    }

    public class VideoStreamCfg {
        /// <summary>
        /// Gets or sets the media format.
        /// </summary>
        public string MediaFormat { get; set; }
        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// Gets or sets the bitrate.
        /// </summary>
        public int Bitrate { get; set; }
        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        public ulong Size { get; set; }
        /// <summary>
        /// Gets or sets the video codec (e.g. h264).
        /// </summary>
        public string Codec { get; set; }
        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Gets or sets the aspect ratio (e.g. 16:9).
        /// </summary>
        public string AspectRatio { get; set; }
        /// <summary>
        /// Gets or sets the pixel format (e.g. you420p).
        /// </summary>
        public string PixelFormat { get; set; }
        /// <summary>
        /// Gets or sets the frame rate (e.g. 30000/1001).
        /// </summary>
         public string FrameRate { get; set; }
    }

    public class AudioStreamCfg {
        /// <summary>
        /// Gets or sets the video codec (e.g. aac).
        /// </summary>
        public string Codec { get; set; }
        /// <summary>
        /// Gets or sets the bitrate.
        /// </summary>
        public int Bitrate { get; set; }
        /// <summary>
        /// Gets or sets the channels.
        /// </summary>
        public string Channels { get; set; }

    }
}

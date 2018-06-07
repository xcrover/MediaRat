using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ops.NetCoe.LightFrame;
using System.IO;

namespace XC.MediaRat {
    /// <summary>
    /// Media Utility
    /// </summary>
    public static class MediaUtil {
        static Dictionary<string, MediaTypes> _mediaExtensions;

        /// <summary>
        /// Gets the media extensions.
        /// </summary>
        /// <value>
        /// The media extensions.
        /// </value>
        public static Dictionary<string, MediaTypes> MediaExtensions {
            get {
                return _mediaExtensions ?? (_mediaExtensions = CreateMediaExtensionMap());
            }
        }


        static Dictionary<string, MediaTypes> CreateMediaExtensionMap() {
            Dictionary<string, MediaTypes> rz = new Dictionary<string, MediaTypes>(StringComparer.OrdinalIgnoreCase);
            foreach (MediaTypes mt in Enum.GetValues(typeof(MediaTypes))) {
                if (mt != MediaTypes.Undefined) {
                    var cfgSrc = AppContext.Current.GetAppCfgItem(string.Format("{0}Extensions", mt));
                    if (cfgSrc != null) {
                        string[] items = cfgSrc.ToLower().Split(new char[] { '|', ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var it in items) {
                            rz["." + it] = mt;
                        }
                    }
                }
            }
            return rz;
        }


        /// <summary>
        /// Gets the type of the file media.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <returns></returns>
        public static MediaTypes GetFileMediaType(string fullPath) {
            string ext = System.IO.Path.GetExtension(fullPath);
            MediaTypes mt;
            if (MediaExtensions.TryGetValue(ext, out mt)) return mt;
            return MediaTypes.Undefined;
        }

        public static KeyValuePairXCol<string, string> GetMetadata(Stream imgStream) {
            KeyValuePairXCol<string, string> rz;
            ExifReader xrf = null;
            try {
                imgStream.Seek(0, SeekOrigin.Begin);
                xrf = new ExifReader(imgStream);
                rz = GetMetadata(xrf);
            }
            finally {
                xrf.Dispose();
            }
            return rz;
        }

        /// <summary>
        /// Get Metadata via ExifReader.
        // XC 20170610: this is real metadata shown on UI
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static KeyValuePairXCol<string, string> GetMetadata(string filePath) {
            KeyValuePairXCol<string, string> rz;
            ExifReader xrf = null;
            try {
                xrf = new ExifReader(filePath);
                rz = GetMetadata(xrf);
            }
            finally {
                if (xrf!=null)
                    xrf.Dispose();
            }
            return rz;
        }

        public static KeyValuePairXCol<string, string> GetMetadata(ImageFile mf) {
            KeyValuePairXCol<string, string> rz;
            ExifReader xrf = null;
            try {
                xrf = new ExifReader(mf.GetFullPath());
                rz = GetMetadata(xrf, mf);
                mf.UpdateTechDescription();
            }
            finally {
                xrf.Dispose();
            }
            return rz;
        }


        public static KeyValuePairXCol<string, string> GetMetadata(ExifReader xfr, ImageFile mf=null) {
            KeyValuePairXCol<string, string> rz = new KeyValuePairXCol<string, string>();
            object tmp;
            string ts;
            UInt32 tw,th;
            DateTime dt;
            StringBuilder sb;
            UInt16 un16;
            double td;
            object otm;
            //CodeValueList mats = (mf==null) ? null : mf.MediaAttributesSafe;
            try {
                sb = new StringBuilder();
                if (xfr.GetTagValue(ExifTags.Make, out ts)) {
                    sb.Append(ts);
                }
                if (xfr.GetTagValue(ExifTags.Model, out ts)) {
                    if (sb.Length > 0) sb.Append(' ');
                    sb.Append(ts);
                }
                if (sb.Length > 0) {
                    rz.Add("Camera", ts=sb.ToString());
                    if (mf != null) mf.Camera = ts;
                }

                if (xfr.GetTagValue(ExifTags.LensModel, out ts)) {
                    rz.Add("Lens", ts);
                    if (mf != null) mf.Lens = ts;
                }

                if (xfr.GetTagValue(ExifTags.XPSubject, out ts)) {
                    rz.Add("Subject", ts);
                }

                if (xfr.GetTagValue(ExifTags.XPTitle, out ts)) {
                    rz.Add("Title", ts);
                }

                if (xfr.GetTagValue(ExifTags.ImageDescription, out ts)) {
                    rz.Add("Description", ts);
                }

                if (xfr.GetTagValue(ExifTags.XPKeywords, out otm)) {
                    byte[] bsrc = otm as byte[];
                    if (bsrc!=null) {
                        //Should be Unicode
                        rz.Add("Keywords", Encoding.Unicode.GetString(bsrc));
                    }
                    else
                        rz.Add("Keywords", otm.ToString());
                }

                if (xfr.GetTagValue(ExifTags.ImageRating, out otm)) {
                    rz.Add("Rating", otm.ToString());
                }

                if (xfr.GetTagValue(ExifTags.ImageRatingPct, out otm)) {
                    rz.Add("Rating [%]", otm.ToString());
                }

                //if (xfr.GetTagValue(Constants.ExifTagId.SonyLens, out ts)) {
                //    rz.Add("Lens", ts);
                //}

                ImageDim dm = GetImgDim(xfr);
                if (dm != null) {
                    if (mf != null) mf.Dimensions = dm;
                    rz.Add("Dimension [px]", dm.ToString());
                }
                //if (xfr.GetTagValue(ExifTags.ImageWidth, out tw)&& xfr.GetTagValue(ExifTags.ImageLength, out th)) {
                //    ImageDim dm = new ImageDim() { Width = tw, Height = th };
                //    rz.Add("Dimension [px]", dm.ToString()); 
                //}
                //else if (xfr.GetTagValue(Constants.ExifTagId.WidthPix, out tw) && xfr.GetTagValue(Constants.ExifTagId.HeightPix, out th)) {
                //    ImageDim dm = new ImageDim() { Width = tw, Height = th };
                //    rz.Add("Dimension [px]", dm.ToString()); 
                //}

                if (xfr.GetTagValue(ExifTags.Orientation, out un16)) {
                    rz.Add("Orientation", un16.ToString());
                }

                if (xfr.GetTagValue(ExifTags.DateTimeDigitized, out dt)) {
                    rz.Add("Date taken", dt.ToString("yyyy-MM-dd HH:mm:ss"));
                    if (mf!=null) mf.Timestamp= dt;
                }

                if (xfr.GetTagValue(ExifTags.FNumber, out td)) {
                    rz.Add("FNumber", td.ToString());
                    if (mf != null) mf.FNumber= td;
                }

                if (xfr.GetTagValue(ExifTags.ApertureValue, out otm)) {
                    rz.Add("Apperture", otm.ToString());
                }

                if (xfr.GetTagValue(ExifTags.ExposureTime, out otm)) {
                    rz.Add("Exposure", ts= otm.ToString());
                    if (mf != null) mf.Exposure= ToDblN(ts);
                }

                if (xfr.GetTagValue(ExifTags.ISOSpeed, out otm)) {
                    rz.Add("ISO Speed", otm.ToString());
                    if (mf != null) mf.Iso= ToIntN(otm);
                }
                else if (xfr.GetTagValue(ExifTags.PhotographicSensitivity, out otm)) {
                    rz.Add("ISO Speed", ts=otm.ToString());
                    if (mf != null) mf.Iso = ToIntN(otm);
                }


                if (xfr.GetTagValue(ExifTags.FocalLength, out otm)) {
                    rz.Add("Focal length", otm.ToString());
                }

                if (xfr.GetTagValue(ExifTags.WhiteBalance, out otm)) {
                    rz.Add("White Balance", otm.ToString());
                }

                if (xfr.GetTagValue(ExifTags.WhitePoint, out otm)) {
                    rz.Add("White Point", otm.ToString());
                }

                //BitmapMetadata mData = imgData as BitmapMetadata;
                //if (mData != null) {

                //    rz.Add("Application", mData.ApplicationName);
                //    rz.Add("Date taken", mData.DateTaken);
                //    rz.Add("Format", mData.Format);
                //    rz.Add("Title", mData.Title);
                //    rz.Add("Subject", mData.Subject);
                //    rz.Add("Location", mData.Location);
                //    rz.Add("Rating", mData.Rating.ToString());
                //    if (mData.Keywords != null) {
                //        rz.Add("Keywords", string.Join(", ", mData.Keywords));
                //    }
                //    rz.Add("Comment", mData.Comment);
                //    //foreach (var ms in mData) {
                //    //    rz.Add("", ms);
                //    //}
                //    //object tmp= mData.GetQuery("/app1/ifd/exif/{ushort=37378}") ?? mData.GetQuery("/xmp/exif:ApertureValue");
                //    //object tmp = mData.GetQuery("System.Photo.Aperture");
                //    //if (null != (tmp = mData.GetQuery("System.Photo.FNumber")))
                //    //    rz.Add("FNumber", tmp.ToString());
                //    //if (null != (tmp = mData.GetQuery("System.Photo.Aperture")))
                //    //    rz.Add("Apperture", tmp.ToString());
                //    //if (null!=(tmp= mData.GetQuery("/app1/ifd/exif/{ushort=33434}") ?? mData.GetQuery("/xmp/exif:ExposureTime")))
                //    //if (null != (tmp = mData.GetQuery("System.Photo.ExposureTime")))
                //    //    rz.Add("Exposure", tmp.ToString());
                //    if (null != (tmp = mData.GetQuery("System.Photo.SubjectDistance")))
                //        rz.Add("Distance", tmp.ToString());
                //    if (null != (tmp = mData.GetQuery("System.Photo.WhiteBalance")))
                //        rz.Add("White balance", tmp.ToString());
                //    //if (null != (tmp = mData.GetQuery("System.Photo.Orientation")))
                //    //    rz.Add("Orientation", tmp.ToString());
                //    //if (null != (tmp = mData.GetQuery("System.Photo.ISOSpeed")))
                //    //    rz.Add("ISO Speed", tmp.ToString());
                //    //if (null != (tmp = mData.GetQuery("System.Photo.FocalLength")))
                //    //    rz.Add("Focal Length", tmp.ToString());
                //    //if (null != (tmp = mData.GetQuery("System.Photo.FocalLengthInFilm")))
                //    //    rz.Add("35mm Focal Length", tmp.ToString());
                //    //DumpMeta(mData);
                //}
            }
            catch (Exception x) {
                throw new BizException(string.Format("Failed to extract properties. {0}: {1}", x.GetType().Name, x.Message));
            }
            return rz;
        }


        public static ImageDim GetImgDim(ExifReader xfr) {
            object tw, th;
            if (xfr.GetTagValue(ExifTags.ImageWidth, out tw) && xfr.GetTagValue(ExifTags.ImageLength, out th)) {
                return new ImageDim() { Width = ToUInt32(tw), Height = ToUInt32(th) };
            }
            else if (xfr.GetTagValue(Constants.ExifTagId.WidthPix, out tw) && xfr.GetTagValue(Constants.ExifTagId.HeightPix, out th)) {
                return new ImageDim() { Width = ToUInt32(tw), Height = ToUInt32(th) };
            }
            return null;
        }

        public static UInt32? GetIso(ExifReader xfr) {
            object otm;
            if (xfr.GetTagValue(ExifTags.ISOSpeed, out otm)|| xfr.GetTagValue(ExifTags.PhotographicSensitivity, out otm)) {
                return ToUInt32(otm);
            }
            return null;
        }

        public static double? ToDblN(object src) {
            if (src == null) return null;
            double t;
            if (double.TryParse(src.ToString(), out t)) {
                return t;
            }
            return null;
        }

        static UInt32 ToUInt32(object src) {
            return Convert.ToUInt32(src);
        }

        static int? ToIntN(object src) {
            if (src == null) return null;
            int t;
            if (int.TryParse(src.ToString(), out t)) {
                return t;
            }
            return null;
        }
    }
}

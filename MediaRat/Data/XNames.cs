using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
    /// <summary>
    /// XML names
    /// </summary>
    public static class XNames {
        /// <summary>
        /// The xn media project
        /// </summary>
        public static XName xnMediaProject = XName.Get("mediaProject");
        /// <summary>
        /// The xn video project
        /// </summary>
        public static XName xnVideoProject = XName.Get("videoProject");
        /// <summary>
        /// The xn Source Media
        /// </summary>
        public static XName xnSourceMedia = XName.Get("sourceMedia");
        /// <summary>
        /// The xn Tracks
        /// </summary>
        public static XName xnTracks = XName.Get("tracks");
        /// <summary>
        /// The xn definitions
        /// </summary>
        public static XName xnDefinitions = XName.Get("definitions");
        /// <summary>
        /// The xn ratings
        /// </summary>
        public static XName xnRatings = XName.Get("ratings");
        /// <summary>
        /// The xn rating
        /// </summary>
        public static XName xnRating = XName.Get("ratingDefinition");
        /// <summary>
        /// The xn categories
        /// </summary>
        public static XName xnCategories = XName.Get("categories");
        /// <summary>
        /// The xn category
        /// </summary>
        public static XName xnCategory = XName.Get("category");
        /// <summary>
        /// The xn vars
        /// </summary>
        public static XName xnVars = XName.Get("variables");
        /// <summary>
        /// The xn variable
        /// </summary>
        public static XName xnVar = XName.Get("var");
        /// <summary>
        /// The xn source filters
        /// </summary>
        public static XName xnSourceFilters = XName.Get("sourceFilters");
        /// <summary>
        /// The xn filter
        /// </summary>
        public static XName xnFilter = XName.Get("filter");
        /// <summary>
        /// The xn sources
        /// </summary>
        public static XName xnMedia = XName.Get("mediaFiles");
        /// <summary>
        /// The xn description
        /// </summary>
        public static XName xnDescription = XName.Get("description");
        /// <summary>
        /// The xn items
        /// </summary>
        public static XName xnItems = XName.Get("items");
        /// <summary>
        /// The xn item
        /// </summary>
        public static XName xnItem = XName.Get("item");
        /// <summary>
        /// The xa identifier
        /// </summary>
        public static XName xaId = XName.Get("id");
        /// <summary>
        /// The xa sourceId
        /// </summary>
        public static XName xaSrcId = XName.Get("srcId");
        ///<summary>The XML name for TrackId [trackId]</summary>
        public static XName xaTrackId = XName.Get("trackId");
        ///<summary>The XML name for Track Group [trackSet]</summary>
        public static XName xnTrackSet = XName.Get("trackSet");
        /// <summary>
        /// The xa name
        /// </summary>
        public static XName xaName = XName.Get("name");
        /// <summary>
        /// The xa value
        /// </summary>
        public static XName xaValue = XName.Get("value");
        /// <summary>
        /// The xa location
        /// </summary>
        public static XName xaLocation = XName.Get("location");
        /// <summary>
        /// The xa type
        /// </summary>
        public static XName xaType = XName.Get("type");
        /// <summary>
        /// The xa pattern
        /// </summary>
        public static XName xaPattern = XName.Get("pattern");
        /// <summary>
        /// The xa маркер
        /// </summary>
        public static XName xaMarker = XName.Get("marker");
        /// <summary>
        /// The xa media format
        /// </summary>
        public static XName xaMediaFormat = XName.Get("mediaFormat");
        /// <summary>
        /// The xa duration
        /// </summary>
        public static XName xaDuration = XName.Get("duration");
        /// <summary>
        /// The xa duration human-friendly text
        /// </summary>
        public static XName xaDurationS = XName.Get("durationS");
        /// <summary>
        /// The xa bitrate
        /// </summary>
        public static XName xaBitrate = XName.Get("bitrate");
        /// <summary>
        /// The xa Width
        /// </summary>
        public static XName xaWidth = XName.Get("width");
        /// <summary>
        /// The xa Height
        /// </summary>
        public static XName xaHeight = XName.Get("height");
        ///<summary>The XML name for StartAt [startAt]</summary>
        public static XName xaStartAt = XName.Get("startAt");
        ///<summary>The XML name for StopAt [stopAt]</summary>
        public static XName xaStopAt = XName.Get("stopAt");
        ///<summary>The XML name for OrderWight [orderw]</summary>
        public static XName xaOrderW = XName.Get("orderw");
        ///<summary>The XML name for Attributes [attributes]</summary>
        public static XName xnAttributes = XName.Get("attributes");

        ///<summary>The XML name for Camera [camera]</summary>
        public static XName xaCamera = XName.Get("camera");
        ///<summary>The XML name for Lens [lens]</summary>
        public static XName xaLens = XName.Get("lens");
        ///<summary>The XML name for Timestamp [timestamp]</summary>
        public static XName xaTimestamp = XName.Get("timestamp");
        ///<summary>The XML name for Exposure [exposure]</summary>
        public static XName xaExposure = XName.Get("exposure");
        ///<summary>The XML name for FNumber [fNumber]</summary>
        public static XName xaFNumber = XName.Get("fNumber");
        ///<summary>The XML name for ISO [iso]</summary>
        public static XName xaIso = XName.Get("iso");

        ///<summary>The XML name for Negate [negate]</summary>
        public static XName xaNegate = XName.Get("negate");

        /// <summary>
        /// Adds the description element.
        /// </summary>
        /// <param name="trg">The TRG.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public static XElement AddDescription(this XElement trg, string description) {
            if (!string.IsNullOrEmpty(description)) {
                trg.Add(new XElement(xnDescription, description));
            }
            return trg;
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="defaultVal">The default value.</param>
        /// <returns></returns>
        public static string GetDescription(this XElement src, string defaultVal = null) {
            XElement xd = src.Element(xnDescription);
            if (xd != null)
                return xd.Value;
            return defaultVal;
        }

        /// <summary>
        /// Deserializes the specified source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">The source.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public static IEnumerable<T> Deserialize<T>(this IEnumerable<XElement> src, string password=null) where T : IXmlConfigurable, new() {
            T rz;
            foreach (var xv in src) {
                rz = new T();
                rz.ApplyConfiguration(xv, password);
                yield return rz;
            }
        }

        /// <summary>
        /// Deserializes the specified source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">The source.</param>
        /// <param name="converter">The converter.</param>
        /// <returns></returns>
        public static IEnumerable<T> Deserialize<T>(this IEnumerable<XElement> src, Func<XElement, T> converter) {
            T rz;
            foreach (var xv in src) {
                rz = converter(xv);
                if (rz!=null)
                    yield return rz;
            }
        }

        /// <summary>
        /// Gets the name of the xa rating.
        /// </summary>
        /// <param name="ratingMarker">The rating marker.</param>
        /// <returns></returns>
        public static XName GetXaRatingName(string ratingMarker) {
            return XName.Get(string.Concat("rt-", ratingMarker??"x34"));
        }
    }
}

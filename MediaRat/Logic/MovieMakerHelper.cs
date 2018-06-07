using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {

    /// <summary>
    /// Helper for project export to Windows Movie Maker project
    /// </summary>
    public class MovieMakerHelper {
        const int MMTypeVideo = 1,
                  MMTypeImage = 2,
                  MMTypeAudio = 3;

        ///<summary>Options</summary>
        private MovieOptions _options;

        ///<summary>Options</summary>
        public MovieOptions Options {
            get { return this._options; }
            set { this._options = value; }
        }

        public MovieMakerHelper(MovieOptions options) {
            this.Options = options;
        }

        /// <summary>
        /// Creates the m maker project.
        /// </summary>
        /// <param name="options">The options.</param>
        public XDocument CreateMMakerProject() {
            XElement xB, xE, xP= new XElement(XNM.xnProject, 
                new XAttribute(XNM.xaName, Options.Name),
                new XAttribute(XNM.xaThemeId, 0),
                new XAttribute(XNM.xaVersion, 65540),
                new XAttribute(XNM.xaTemplateId, "SimpleProjectTemplate"));
            XDocument xrz = new XDocument(new XDeclaration("1.0", "utf-8", string.Empty), xP);
            xP.Add(xB=new XElement(XNM.xnMediaItems),
                xE= new XElement(XNM.xnExtents),
                GetBoundPlaceholdersXml(),
                GetProjectBoundProps(),
                GetThemeOperationLogXml(),
                GetAudioDuckingPropertiesXml());
            for (int i = 0; i < Options.MediaFiles.Count; i++) {
                xB.Add(this.GetMediaItemXml(GetMediaItemId(i), Options.MediaFiles[i]));
                xE.Add(this.GetExtentXml(i));
            }
            for (int i = 1; i < 5; i++) {
                if (i == 1) {
                    xE.Add(this.GetExtentSelectorXml(i, true));
                }
                else
                    xE.Add(this.GetExtentSelectorXml(i));
            }
            return xrz;
        }

        XElement GetMediaItemXml(int id, MediaFile mf) {
            var gauge = GetGauge(mf);
            var sDuration = GetDurationS(mf).ToString();
            int mType;
            switch(mf.MediaType) {
                case MediaTypes.Video: mType= MMTypeVideo; break;
                case MediaTypes.Audio: mType= MMTypeAudio; break;
                case MediaTypes.Image: mType= MMTypeImage; break;
                default: throw new ArgumentException(string.Format("Media type {1} is not supported by Movie Maker. Item: \"{0}\"", mf.Title, mf.MediaType));
            }
            XElement xrz = new XElement(XNM.xnMediaItem,
                new XAttribute(XNM.xaId, id),
                new XAttribute(XNM.xaFilePath, mf.FullName),
                new XAttribute(XNM.xaWidth, gauge.Item1),
                new XAttribute(XNM.xaHeight, gauge.Item2),
                new XAttribute(XNM.xaDuration, sDuration),

                new XAttribute(XNM.xaSongTitle, string.Empty),
                new XAttribute(XNM.xaSongArtist, string.Empty),
                new XAttribute(XNM.xaSongAlbum, string.Empty),
                new XAttribute(XNM.xaSongCopyrightUrl, string.Empty),
                new XAttribute(XNM.xaSongArtistUrl, string.Empty),
                new XAttribute(XNM.xaSongAudioFileUrl, string.Empty),

                new XAttribute(XNM.xaStabilization, 0),
                new XAttribute(XNM.xaMediaItemType, mType));

            return xrz;
        }

        XElement GetExtentXml(int mfIx) {
            MediaFile mf= Options.MediaFiles[mfIx];
            XName xType;
            switch(mf.MediaType) {
                case MediaTypes.Video: xType= XNM.xnVideoClip; break;
                case MediaTypes.Audio: xType= XNM.xnAudioClip; break;
                case MediaTypes.Image: xType=XNM.xnImageClip; break;
                default: throw new ArgumentException(string.Format("Media type {1} is not supported by Movie Maker. Item: \"{0}\"", mf.Title, mf.MediaType));
            }

            XElement xrz = new XElement(xType,
                new XAttribute(XNM.xaExtentId, GetExtentId(mfIx)),
                new XAttribute(XNM.xaGapBefore, 0),
                new XAttribute(XNM.xaMediaItemId, GetMediaItemId(mfIx)));

            switch (mf.MediaType) {
                case MediaTypes.Audio:
                case MediaTypes.Video: AddVideoExtentAttrs(xrz); break;
                case MediaTypes.Image: AddImgExtentAttrs(xrz); break;
            }

            xrz.Add(new XElement(XNM.xnEffects),
                new XElement(XNM.xnTransitions, GetTransitionCinemaBlurXml()),
                GetImgBoundProps(mf));
            // Add default transition

            return xrz;
        }

        void AddVideoExtentAttrs(XElement xExt) {
            xExt.Add(new XAttribute("inTime", "0"),
                new XAttribute("outTime", "0"),
                new XAttribute("speed", "1"),
                new XAttribute("stabilizationMode", "0")
                );
        }

        void AddImgExtentAttrs(XElement xExt) {
            xExt.Add(new XAttribute(XNM.xaDuration, Options.ImgDisplayTime));
        }


        XElement GetExtentSelectorXml(int extentId, bool isPrymaryTrack = false) {
            XElement xrfs, xrz = new XElement(XNM.xnExtentSelector,
                new XAttribute(XNM.xaExtentId, extentId),
                new XAttribute(XNM.xaGapBefore,0),
                new XAttribute(XNM.xaPrimaryTrack, isPrymaryTrack),
                new XElement(XNM.xnEffects),
                new XElement(XNM.xnTransitions),
                new XElement(XNM.xnBoundProperties),
                xrfs= new XElement(XNM.xnExtentRefs));
            if (isPrymaryTrack) { // Add references to actual image extents
                for (int i = 0; i < this.Options.MediaFiles.Count; i++) {
                    xrfs.Add(new XElement(XNM.xnExtentRef,
                        new XAttribute(XNM.xaId, GetExtentId(i))));
                }
            }
            return xrz;
        }

        XElement GetBoundPlaceholdersXml() {
            XElement xrz = new XElement(XNM.xnBoundPlaceholders,
                new XElement(XNM.xnBoundPlaceholder,
                    new XAttribute(XNM.xaPlaceholderId, "SingleExtentView"),
                    new XAttribute(XNM.xaExtentId, 0)),
                new XElement(XNM.xnBoundPlaceholder,
                    new XAttribute(XNM.xaPlaceholderId, "Main"),
                    new XAttribute(XNM.xaExtentId, 1)),
                new XElement(XNM.xnBoundPlaceholder,
                    new XAttribute(XNM.xaPlaceholderId, "SoundTrack"),
                    new XAttribute(XNM.xaExtentId, 2)),
                new XElement(XNM.xnBoundPlaceholder,
                    new XAttribute(XNM.xaPlaceholderId, "Narration"),
                    new XAttribute(XNM.xaExtentId, 3)),
                new XElement(XNM.xnBoundPlaceholder,
                    new XAttribute(XNM.xaPlaceholderId, "Text"),
                    new XAttribute(XNM.xaExtentId, 4)));
            return xrz;
        }

        XElement GetProjectBoundProps() {
            XElement xrz = new XElement(XNM.xnBoundProperties,
                new XElement(XNM.xnBoundPropertyFloatSet,
                    new XAttribute(XNM.xaBPName, "AspectRatio"),
                    new XElement(XNM.xnBoundPropertyFloatElement,
                        new XAttribute(XNM.xaBPValue, "1.7777776718139648"))),
                GetBoundPropXml(XNM.xnBoundPropertyFloat, "DuckedNarrationAndSoundTrackMix", "0.5"),
                GetBoundPropXml(XNM.xnBoundPropertyFloat, "DuckedVideoAndNarrationMix", "0.5"),
                GetBoundPropXml(XNM.xnBoundPropertyFloat, "DuckedVideoAndSoundTrackMix", "0.5"),
                GetBoundPropXml(XNM.xnBoundPropertyFloat, "SoundTrackMix", "0.5"));
            return xrz;
        }

        XElement GetBoundPropXml(XName bpName, string name, object val) {
            return new XElement(bpName,
                new XAttribute(XNM.xaBPName, name),
                new XAttribute(XNM.xaBPValue, val));
        }

        XElement GetImgBoundProps(MediaFile mf) {
            var xrz = new XElement(XNM.xnBoundProperties);
            switch (mf.MediaType) {
                case MediaTypes.Video:
                    xrz.Add(GetBoundPropXml(XNM.xnBoundPropertyBool, "Mute", "false"));
                    xrz.Add(GetBoundPropXml(XNM.xnBoundPropertyInt, "rotateStepNinety", "0"));
                    xrz.Add(GetBoundPropXml(XNM.xnBoundPropertyFloat, "Volume", "1"));
                    break;
                default:
                    xrz.Add(GetBoundPropXml(XNM.xnBoundPropertyInt, "rotateStepNinety", "0"));
                    break;
            }
            return xrz;
        }

        XElement GetThemeOperationLogXml() {
            return new XElement(XNM.xnThemeOperationLog,
                new XAttribute("themeID", 0),
                new XElement(XNM.xnMonolithicThemeOperations));
        }

        XElement GetAudioDuckingPropertiesXml() {
            return new XElement(XName.Get("AudioDuckingProperties"),
                new XAttribute("emphasisPlaceholderID", "Narration"));
        }

        XElement GetTransitionCinemaBlurXml() {
            return new XElement(XNM.xnShapeEffect,
                new XAttribute("effectTemplateID", "CinematicBlurTransitionTemplate"),
                new XAttribute(XNM.xaDuration, "1.5"),
                new XElement(XNM.xnBoundProperties));
        }


        /// <summary>
        /// Gets the media item identifier. Calculated as <paramref name="mfIx"/>+1.
        /// </summary>
        /// <param name="mfIx">Media file index in the array</param>
        /// <returns></returns>
        int GetMediaItemId(int mfIx) {
            return mfIx + 1;
        }

        /// <summary>
        /// Extent Id from 0 to 4 are allocated for ExtentSelectors (timelines) so
        /// media file extent Ids must start from 5.
        /// </summary>
        /// <param name="mfIx">Media file index in the array</param>
        /// <returns></returns>
        int GetExtentId(int mfIx) {
            return mfIx + 5;
        }

        Tuple<int, int> GetGauge(MediaFile mf) {
            switch (mf.MediaType) {
                case MediaTypes.Image: return new Tuple<int, int>(4912, 2760);
                case MediaTypes.Video: return new Tuple<int, int>(1920, 1080);
                default: return new Tuple<int, int>(0, 0);
            }
        }

        double GetDurationS(MediaFile mf) {
            switch (mf.MediaType) {
                //case MediaTypes.Audio: {
                //    AudioFile af= (AudioFile)mf;
                //}
                case MediaTypes.Video: {
                    VideoFile vf = (VideoFile)mf;
                    return vf.Duration.HasValue ? vf.Duration.Value.TotalSeconds : 0.0;
                    }
                default: return 0.0;
            }
        }


        static class XNM {
            public static XName xnProject = XName.Get("Project");
            public static XName xnMediaItems = XName.Get("MediaItems");
            public static XName xnMediaItem = XName.Get("MediaItem");
            public static XName xaName = XName.Get("name");
            public static XName xaId = XName.Get("id");
            public static XName xaExtentId = XName.Get("extentID");
            public static XName xaMediaItemId = XName.Get("mediaItemID");
            public static XName xaMediaItemType = XName.Get("mediaItemType");
            public static XName xaFilePath = XName.Get("filePath");
            public static XName xaVersion = XName.Get("version");
            public static XName xaThemeId = XName.Get("themeId");
            public static XName xaTemplateId = XName.Get("templateID");
            public static XName xaWidth = XName.Get("arWidth");
            public static XName xaHeight = XName.Get("arHeight");
            public static XName xaDuration = XName.Get("duration");
            public static XName xaStabilization = XName.Get("stabilizationMode");
            public static XName xaGapBefore = XName.Get("gapBefore");
            public static XName xaPrimaryTrack = XName.Get("primaryTrack");


            public static XName xaSongTitle = XName.Get("songTitle");
            public static XName xaSongArtist = XName.Get("songArtist");
            public static XName xaSongAlbum = XName.Get("songAlbum");
            public static XName xaSongCopyrightUrl = XName.Get("songCopyrightUrl");
            public static XName xaSongArtistUrl = XName.Get("songArtistUrl");
            public static XName xaSongAudioFileUrl = XName.Get("songAudioFileUrl");

            public static XName xnExtents = XName.Get("Extents");
            public static XName xnVideoClip = XName.Get("VideoClip");
            public static XName xnImageClip = XName.Get("ImageClip");
            public static XName xnAudioClip = XName.Get("AudioClip");
            public static XName xnExtentSelector = XName.Get("ExtentSelector");

            public static XName xnEffects = XName.Get("Effects");
            public static XName xnTransitions = XName.Get("Transitions");
            public static XName xnExtentRefs = XName.Get("ExtentRefs");
            public static XName xnExtentRef = XName.Get("ExtentRef");
            public static XName xnBoundProperties = XName.Get("BoundProperties");
            public static XName xnBoundPropertyInt = XName.Get("BoundPropertyInt");
            public static XName xnBoundPropertyBool = XName.Get("BoundPropertyBool");
            public static XName xnBoundPropertyFloat = XName.Get("BoundPropertyFloat");
            public static XName xnBoundPropertyFloatSet = XName.Get("BoundPropertyFloatSet");
            public static XName xnBoundPropertyFloatElement = XName.Get("BoundPropertyFloatElement");
            public static XName xaBPName = XName.Get("Name");
            public static XName xaBPValue = XName.Get("Value");

            public static XName xnBoundPlaceholders = XName.Get("BoundPlaceholders");
            public static XName xnBoundPlaceholder = XName.Get("BoundPlaceholder");
            public static XName xaPlaceholderId = XName.Get("placeholderID");

            public static XName xnThemeOperationLog = XName.Get("ThemeOperationLog");
            public static XName xnMonolithicThemeOperations = XName.Get("MonolithicThemeOperations");

            public static XName xnShapeEffect = XName.Get("ShapeEffect");
            
        }

        /// <summary>
        /// Option for Movie Maker Project
        /// </summary>
        public class MovieOptions : NotifyPropertyChangedBase {
            ///<summary>Target file name</summary>
            private string _targetFileName;
            ///<summary>Image display time</summary>
            private double _imgDisplayTime;
            ///<summary>Collection of Media Files</summary>
            private IList<MediaFile> _mediaFiles;
            ///<summary>Project Name</summary>
            private string _name;

            ///<summary>Project Name</summary>
            public string Name {
                get { return this._name; }
                set {
                    if (this._name != value) {
                        this._name = value;
                        this.FirePropertyChanged("Name");
                    }
                }
            }
            

            ///<summary>Collection of Media Files</summary>
            public IList<MediaFile> MediaFiles {
                get { return (this._mediaFiles==null) ? (this._mediaFiles= new List<MediaFile>()) : this._mediaFiles; }
                set { this._mediaFiles = value; }
            }
            

            ///<summary>Image display time</summary>
            public double ImgDisplayTime {
                get { return this._imgDisplayTime; }
                set {
                    if (this._imgDisplayTime != value) {
                        this._imgDisplayTime = value;
                        this.FirePropertyChanged("ImgDisplayTime");
                    }
                }
            }


            ///<summary>Target file name</summary>
            public string TargetFileName {
                get { return this._targetFileName; }
                set {
                    if (this._targetFileName != value) {
                        this._targetFileName = value;
                        this.FirePropertyChanged("TargetFileName");
                    }
                }
            }
            
        }
    }
}

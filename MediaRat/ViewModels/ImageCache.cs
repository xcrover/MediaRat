using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Ops.NetCoe.LightFrame;

namespace XC.MediaRat {
    /// <summary>
    /// Image cache
    /// Read video file props: http://stackoverflow.com/questions/220097/read-write-extended-file-properties-c
    /// </summary>
    public class ImageCache : NotifyPropertyChangedBase {
        /////<summary>Current Media path</summary>
        //private string _currentPath;
        /////<summary>Current Image</summary>
        //private ImageSource _currentImage;
        ///<summary>Thread Dispatcher for the UI thread. Automatically initialized to the thread on which this instance has been created.</summary>
        private Dispatcher _uiDispatcher = Dispatcher.CurrentDispatcher;
        ///<summary>Status</summary>
        private IMessagePresenter _status;
        ///<summary>Current Image</summary>
        private ImageData _currentImage;

        ///<summary>Current Image</summary>
        public ImageData CurrentImage {
            get { return this._currentImage; }
            set {
                if (this._currentImage != value) {
                    this._currentImage = value;
                    this.FirePropertyChanged("CurrentImage");
                }
            }
        }
        

        ///<summary>Status</summary>
        public IMessagePresenter Status {
            get { return this._status; }
            set { this._status = value; }
        }
                

        ///<summary>Thread Dispatcher for the UI thread. Automatically initialized to the thread on which this instance has been created.</summary>
        public Dispatcher UIDispatcher {
            get { return this._uiDispatcher; }
            set { this._uiDispatcher = value; }
        }

        /////<summary>Current Image</summary>
        //public ImageSource CurrentImage {
        //    get { return this._currentImage; }
        //    set {
        //        if (this._currentImage != value) {
        //            this._currentImage = value;
        //            this.FirePropertyChanged("CurrentImage");
        //        }
        //    }
        //}
        

        /////<summary>Current Media path</summary>
        //public string CurrentPath {
        //    get { return this._currentPath; }
        //    set {
        //        if (this._currentPath != value) {
        //            this._currentPath = value;
        //            this.FirePropertyChanged("CurrentPath");
        //            this.EnsureCurrentImage(this._currentPath);
        //        }
        //    }
        //}

        /// <summary>
        /// Ensures that the specified operation is performed on UI thread.
        /// This method uses <see cref="UIDispatcher"/> to invoke the method.
        /// </summary>
        /// <param name="act">The act.</param>
        public void RunOnUIThread(Action act) {
            if (this.UIDispatcher.CheckAccess())
                act();
            else
                this.UIDispatcher.BeginInvoke(act, DispatcherPriority.Normal);
        }

        /// <summary>
        /// Fires the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        override public void FirePropertyChanged(string propertyName) {
            this.RunOnUIThread(() => base.FirePropertyChanged(propertyName));
        }

        //ImageData GetImage(string fileName) {
        //    if (string.IsNullOrEmpty(fileName)) return null;
        //    ImageData rz = new ImageData() { FileName = fileName };
        //    BitmapSource img = null;
        //    try {
        //        byte[] buffer = File.ReadAllBytes(fileName);
        //        MemoryStream memoryStream = new MemoryStream(buffer);
        //        img = BitmapFrame.Create(memoryStream);
        //        rz.MediaProps = GetMetadata(img.Metadata as BitmapMetadata);
        //        img.Freeze();
        //        rz.Content = img;
        //    }
        //    catch (Exception x) {
        //        this.Status.SetError(string.Format("Failed to load image from \"{0}\". {1}: {2}", fileName, x.GetType().Name, x.Message));
        //    }
        //    return rz;
        //}

        //ImageSource GetImage(string fileName) {
        //    if (string.IsNullOrEmpty(fileName)) return null;
        //    BitmapSource img = null;
        //    try {
        //        FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        //        img = BitmapFrame.Create(fs);
        //        this.MediaProps = GetMetadata(img.Metadata as BitmapMetadata);
        //        img.Freeze();
        //        return img;
        //    }
        //    catch (Exception x) {
        //        this.Status.SetError(string.Format("Failed to load image from \"{0}\". {1}: {2}", fileName, x.GetType().Name, x.Message));
        //    }
        //    return img;
        //}


         //ImageSource GetImage(string fileName) {
         //    if (string.IsNullOrEmpty(fileName)) return null;
         //    BitmapImage bitmap = null;
         //    try {
         //        byte[] buffer = File.ReadAllBytes(fileName);
         //        MemoryStream memoryStream = new MemoryStream(buffer);
         //        bitmap = new BitmapImage();
         //        bitmap.BeginInit();
         //        //bitmap.DecodePixelWidth = 80;
         //        //bitmap.DecodePixelHeight = 60;
         //        bitmap.StreamSource = memoryStream;
         //        bitmap.StreamSource = fs;
         //        bitmap.EndInit();
         //        bitmap.Freeze();
         //    }
         //    catch (Exception x) {
         //        this.Status.SetError(string.Format("Failed to load image from \"{0}\". {1}: {2}", fileName, x.GetType().Name, x.Message));
         //    }
         //    return bitmap;
         //}


         /////<summary>Media Properties</summary>
         //private KeyValuePairXCol<string, string> _mediaProps;

         /////<summary>Media Properties</summary>
         //public KeyValuePairXCol<string, string> MediaProps {
         //    get { return this._mediaProps; }
         //    set {
         //        if (this._mediaProps != value) {
         //            this._mediaProps = value;
         //            this.FirePropertyChanged("MediaProps");
         //        }
         //    }
         //}


         //public KeyValuePairXCol<string, string> GetMetadata(ImageMetadata imgData) {
         //    KeyValuePairXCol<string, string> rz = new KeyValuePairXCol<string, string>();
         //    try {
         //        BitmapMetadata mData = imgData as BitmapMetadata;
         //        if (mData != null) {
         //            rz.Add("Camera", string.Format("{1} [{0}]", mData.CameraManufacturer, mData.CameraModel));
         //            rz.Add("Application", mData.ApplicationName);
         //            rz.Add("Date taken", mData.DateTaken);
         //            rz.Add("Format", mData.Format);
         //            rz.Add("Title", mData.Title);
         //            rz.Add("Subject", mData.Subject);
         //            rz.Add("Location", mData.Location);
         //            rz.Add("Rating", mData.Rating.ToString());
         //            if (mData.Keywords != null) {
         //                rz.Add("Keywords", string.Join(", ", mData.Keywords));
         //            }
         //            rz.Add("Comment", mData.Comment);
         //            //foreach (var ms in mData) {
         //            //    rz.Add("", ms);
         //            //}
         //            //object tmp= mData.GetQuery("/app1/ifd/exif/{ushort=37378}") ?? mData.GetQuery("/xmp/exif:ApertureValue");
         //            object tmp = mData.GetQuery("System.Photo.Aperture");
         //            if (null != (tmp = mData.GetQuery("System.Photo.FNumber")))
         //                rz.Add("FNumber", tmp.ToString());
         //            if (tmp != null)
         //                rz.Add("Apperture", tmp.ToString());
         //            //if (null!=(tmp= mData.GetQuery("/app1/ifd/exif/{ushort=33434}") ?? mData.GetQuery("/xmp/exif:ExposureTime")))
         //            if (null != (tmp = mData.GetQuery("System.Photo.ExposureTime")))
         //                rz.Add("Exposure", tmp.ToString());
         //            if (null != (tmp = mData.GetQuery("System.Photo.SubjectDistance")))
         //                rz.Add("Distance", tmp.ToString());
         //            if (null != (tmp = mData.GetQuery("System.Photo.WhiteBalance")))
         //                rz.Add("White balance", tmp.ToString());
         //            if (null != (tmp = mData.GetQuery("System.Photo.Orientation")))
         //                rz.Add("Orientation", tmp.ToString());
         //            if (null != (tmp = mData.GetQuery("System.Photo.ISOSpeed")))
         //                rz.Add("ISO Speed", tmp.ToString());
         //            if (null != (tmp = mData.GetQuery("System.Photo.FocalLength")))
         //                rz.Add("Focal Length", tmp.ToString());

         //        }
         //    }
         //    catch (Exception x) {
         //        this.Status.SetError(string.Format("Failed to extract properties. {0}: {1}", x.GetType().Name, x.Message));

         //    }
         //    return rz;
         //}


         //void EnsureCurrentImage(string filePath) {
         //    this.CurrentImage = GetImage(filePath);
         //}

         void EnsureCurrentVideo(VideoFile mf) {
             if (mf == null)
                 this.CurrentImage = null;
             else {
                 ImageData rz = new ImageData() { FileName = mf.FullName };
                 rz.MediaProps = mf.GetMetadata();
                 this.CurrentImage = rz;
             }
         }

         /// <summary>
         /// Sets the media file.
         /// </summary>
         /// <param name="mediaFile">The media file.</param>
         public void SetMediaFile(MediaFile mediaFile) {
             if (mediaFile == null)
                 this.CurrentImage = null;
             else if (mediaFile.MediaType == MediaTypes.Image) {
                 ImageFile tmp = mediaFile as ImageFile;
                 if (tmp == null)
                     this.CurrentImage = null;
                 else
                     this.CurrentImage = tmp.GetImageData();
             }
             else if (mediaFile.MediaType == MediaTypes.Video) {
                 EnsureCurrentVideo(mediaFile as VideoFile);
             }
             else {
                 this.CurrentImage = null;
             }
         }
    }

}

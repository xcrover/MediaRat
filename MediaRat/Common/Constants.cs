using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XC.MediaRat {
    public static partial class Constants {
        public static class ExifQuery {
            public static string Base = "/app1/ifd/exif";
            public static string SonyLens = Base + "/{ushort=42036}";
            public static string WidthPix = Base + "/{ushort=40962}"; // 0xA002
            public static string HeightPix = Base + "/{ushort=40963}"; // 0xA003
        }

        public static class ExifTagId {
            public static ushort SonyLens = 42036;
            public static ushort WidthPix = 40962; // 0xA002
            public static ushort HeightPix = 40963; // 0xA003

        }
    }
}

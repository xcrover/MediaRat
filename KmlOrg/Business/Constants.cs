using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmlOrg {
    public static class Constants {
        public static class StyleIds {
            public const string Base = "#placemark";
            public const string Blue = Base + "-blue";
            public const string Brown = Base + "-brown";
            public const string Green = Base + "-green";
            public const string Orange = Base + "-orange";
            public const string Pink = Base + "-pink";
            public const string Purple = Base + "-purple";
            public const string Red = Base + "-red";
            public const string Yellow = Base + "-yellow";

            public static List<string> GetItems() {
                return new List<string> {
                    Blue, Brown, Green, Orange, Pink, Purple, Red, Yellow
                };
            }
        }
    }

    public enum GeoFormats {
        KML,
        GPX
    }
}

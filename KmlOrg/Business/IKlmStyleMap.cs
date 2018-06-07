using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ops.NetCoe.LightFrame;

namespace KmlOrg.Business {
    public interface IKlmStyleMap {
        string GetStyleByMarker(string marker);
        string GetMarkerByStyle(string style);
        List<CodeValuePair> Markers { get; }
    }
}

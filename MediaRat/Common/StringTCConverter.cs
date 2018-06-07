using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace XC.MediaRat {
    /// <summary>
    /// Converts string to timecode
    /// </summary>
    public class StringTCConverter : IValueConverter {

        #region IValueConverter Members

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            TimeSpan? ts = value as TimeSpan?;
            if (ts == null) return null;
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Value.Hours, ts.Value.Minutes, ts.Value.Seconds, ts.Value.Milliseconds);
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return null;
            string[] parts = value.ToString().Split(':');
            if (parts.Length > 3) throw new FormatException(string.Format("Wrong Time Code format. Expected HH:mm:ss.ff"));
            if (parts.Length == 0) return null;
            int pn = parts.Length - 1;
            double secs = double.Parse(parts[pn]);
            pn--;
            if (pn >= 0) {
                secs += double.Parse(parts[pn]) * 60;
                pn--;
                if (pn >= 0) {
                    secs += double.Parse(parts[pn]) * 3600;
                }
            }
            return (TimeSpan?)TimeSpan.FromSeconds(secs);
        }

        #endregion
    }

}

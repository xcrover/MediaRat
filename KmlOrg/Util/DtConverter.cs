using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KmlOrg {
    /// <summary>
    /// Converts string to DateTime
    /// </summary>
    public class DtConverter : IValueConverter {
        public bool ExcludeDate { get; set; }

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
            if (value == null) return null;
            DateTime dt = (DateTime)value;
            if (this.ExcludeDate) {
                return string.Format("{0:HH:mm:ss}", dt);
            }
            else {
                return string.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
            }
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
            DateTime dt;
            string sval = value.ToString();
            if (DateTime.TryParse(sval, out dt)) {
                return dt;
            }
            else {
                throw new FormatException(string.Format("Wrong DateTime format. Expected yyyy-MM-dd HH:mm:ss"));
            }
        }

        #endregion
    }
}

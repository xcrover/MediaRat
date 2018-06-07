using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KmlOrg {
    /// <summary>
    /// Interface with basic UI elements
    /// </summary>
    public interface IUIHelper {
        /// <summary>
        /// Gets the main window.
        /// </summary>
        /// <returns></returns>
        MainWindow GetMainWindow();

        /// <summary>
        /// Selects the folder.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="initialFolder">The initial folder.</param>
        /// <returns></returns>
        bool SelectFolder(string description, ref string initialFolder);

        bool TrySelectFiles(FileAccessRequest far, Action<string> apply);

        /// <summary>
        /// Gets the user confirmation.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <returns></returns>
        bool GetUserConfirmation(string question);

        //bool TryAskText(string title, string label, string initVal, Action<string> newVal);
        WellKnownResponds GetStdRespond(string question);
    }

    public enum WellKnownResponds {
        Yes,
        No,
        Cancel
    }

    /// <summary>
    /// Request to access file via Open or Save File dialog
    /// </summary>
    public class FileAccessRequest {

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAccessRequest"/> class.
        /// </summary>
        public FileAccessRequest() {
            SuggestedFileName = string.Empty;
            ExtensionFilter = "All files (*.*)|*.*";
        }

        /// <summary><c>True</c> if access requested for reading; otherwise for writing.</summary>
        public bool IsForReading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether more than one file can be selected for reading.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if more than one file can be selected for reading; otherwise, <c>false</c>.
        /// </value>
        public bool IsMultiSelect { get; set; }

        /// <summary>
        /// Gets or sets the suggested file name. Can be <c>null</c> or empty.
        /// </summary>
        /// <value>The suggested file name.</value>
        public string SuggestedFileName { get; set; }

        /// <summary>
        /// Gets or sets the extension filter in the same format as it is used in the Open|SaveFileDialog:
        /// <para>"XML documents (*.xml)|*.xml|Text documents (*.txt)|*.txt|All files (*.*)|*.*"</para>
        /// </summary>
        /// <value>The extension filter.</value>
        public string ExtensionFilter { get; set; }

        /// <summary>
        /// Gets or sets the <c>1-based</c> index of the default entry in the extension filter.
        /// </summary>
        /// <value>The index of the default entry in the extension filter.</value>
        public int ExtensionFilterIndex { get; set; }

    }
}

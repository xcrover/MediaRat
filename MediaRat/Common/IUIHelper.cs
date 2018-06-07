using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XC.MediaRat {
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

        /// <summary>
        /// Gets the user confirmation.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <returns></returns>
        bool GetUserConfirmation(string question);

        bool TryAskText(string title, string label, string initVal, Action<string> newVal);

    }
}

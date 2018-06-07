using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace XC.MediaRat {
    /// <summary>
    /// Shell Command
    /// </summary>
    public static class OSCommand {

        /// <summary>
        /// Runs the specified tool.
        /// </summary>
        /// <param name="tool">The tool.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="stdOutHandler">The STD out handler.</param>
        /// <param name="stdErrHandler">The STD err handler.</param>
        /// <returns></returns>
        public static Process Run(string tool, string arguments = null, Action<string> stdOutHandler = null, Action<string> stdErrHandler = null) {
            Process pcs = new Process();
            pcs.StartInfo.FileName = tool;
            if (!string.IsNullOrEmpty(arguments))
                pcs.StartInfo.Arguments = arguments;
            pcs.StartInfo.UseShellExecute = false;
            pcs.StartInfo.RedirectStandardError = true;
            pcs.StartInfo.RedirectStandardOutput = true;
            pcs.StartInfo.CreateNoWindow = true;
            if (stdErrHandler == null) {
                stdErrHandler = (s) => {
                    System.Diagnostics.Debug.Write("ERR:>>"); System.Diagnostics.Debug.WriteLine(s);
                };
            }
            if (stdOutHandler == null) {
                stdOutHandler = (s) => {
                    System.Diagnostics.Debug.Write("OUT:>>"); System.Diagnostics.Debug.WriteLine(s);
                };
            }
            pcs.ErrorDataReceived += (s, e) => stdErrHandler(e.Data);
            pcs.OutputDataReceived += (s, e) => stdOutHandler(e.Data);
            pcs.Start();
            pcs.BeginErrorReadLine();
            pcs.BeginOutputReadLine();
            return pcs;
        }

        public static Process RunShellCmd(string tool, string arguments = null) {
            Process pcs = new Process();
            pcs.StartInfo.FileName = tool;
            if (!string.IsNullOrEmpty(arguments))
                pcs.StartInfo.Arguments = arguments;
            pcs.StartInfo.UseShellExecute = true;
            //pcs.StartInfo.RedirectStandardError = true;
            //pcs.StartInfo.RedirectStandardOutput = true;
            pcs.StartInfo.CreateNoWindow = false;
            pcs.Start();
            return pcs;
        }

        /// <summary>
        /// Runs the and wait.
        /// </summary>
        /// <param name="tool">The tool.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="stdOutHandler">The STD out handler.</param>
        /// <param name="stdErrHandler">The STD err handler.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns></returns>
        public static Process RunAndWait(string tool, string arguments = null, Action<string> stdOutHandler = null, Action<string> stdErrHandler = null, int timeout=15000) {
            Process pcs = Run(tool, arguments, stdOutHandler, stdErrHandler);
            pcs.WaitForExit(timeout);
            return pcs;
        }



    }
}

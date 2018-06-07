using Ops.NetCoe.LightFrame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace XC.MediaRat {
    /// <summary>
    /// External command (e.g. ffMpeg) with predefined parameters
    /// </summary>
    public class ExternalCommand {
        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ToolTemplate { get; set; }
        public string ToolArgsTemplate { get; set; }
        public HashSet<string> ApplicableFileExtensions { get; set; }
        public int TimeoutSec { get; set; }

        public static ExternalCommand CreateByCfg(string cfgText) {
            ExternalCommand rz = new ExternalCommand();
            rz.ApplyCfg(cfgText);
            return rz;
        }

        public ExternalCommand() {
            this.TimeoutSec = 240;
        }

        /// <summary>
        /// Apply configuration text
        /// </summary>
        /// <param name="cfgText">
        /// <paramref name="cfgText"/> consists of 5 elements separated by "|". 
        /// 1. Menu title
        /// 2. Description
        /// 3. Applicability filter(list of file extensions separated by on of ";|"
        /// 4. Application to call
        /// 5. Application parameters
        ///Each element tries template substitution for key specified as {?key ?}.
        /// </param>
        void ApplyCfg(string cfgText) {
            string[] cis = cfgText.Split('|');
            if (cis.Length != 6)
                throw new BizException("External cmd config must consist of 6 |-separated elements: title|description|timeoutSec|applicableExtensions|tool|arguments.\r\n Configuration: " + cfgText);
            TextStencil tt = new TextStencil() { FindParamValue = GetVal };
            int timeoutS;
            this.Title = tt.Transform(cis[0]);
            this.Description = tt.Transform(cis[1]);
            if (!int.TryParse(cis[2], out timeoutS)) {
                this.TimeoutSec = 60;
            }
            else {
                this.TimeoutSec = timeoutS;
            }
            this.ApplicableFileExtensions = null;
            if (!string.IsNullOrWhiteSpace(cis[3])) {
                var tmp = tt.Transform(cis[3]);
                if (!string.IsNullOrWhiteSpace(tmp)) {
                    this.ApplicableFileExtensions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                    string[] xts = tmp.Split(';', ',', '|');
                    (from k in xts where !string.IsNullOrWhiteSpace(k) select k).Apply((x) => this.ApplicableFileExtensions.Add(x));
                }
            }
            this.ToolTemplate = cis[4];
            this.ToolArgsTemplate = cis[5]; // 
        }

        /// <summary>
        /// Checks if this command can be applied to the target media file.
        /// <paramref name="srf"/> must implement <see cref="ISourceRef"/>.
        /// </summary>
        /// <param name="srf">Must implement <see cref="ISourceRef"/></param>
        /// <returns></returns>
        public virtual bool CanExecute(ISourceRef srf) {
            if (srf == null) return false;
            if (this.ApplicableFileExtensions!=null) {
                if (!this.ApplicableFileExtensions.Contains(TemplParams.GetFileExt(srf.SourcePath)))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Default get value for substitution method. Just grabs from app.config if <paramref name="key"/> starts from <c>appCfg:</c>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string GetVal(string key) {
            const string pfxAppCfg = "appCfg:";
            if (key.StartsWith(pfxAppCfg)) {
                return AppContext.Current.GetAppCfgItem(key.Substring(pfxAppCfg.Length));
            }
            return null;
        }

        /// <summary>
        /// Execute external command. <paramref name="srf"/> must implement <see cref="ISourceRef"/>.
        /// </summary>
        /// <param name="srf">Target media file. Must implement <see cref="ISourceRef"/></param>
        public virtual void Execute(ISourceRef srf, Func<string, string> getValue = null) {
            if (srf!=null) {
                this.Execute(TemplParams.EnumerateStdSourceParams(srf), getValue);
            }
        }

        public virtual void Execute(IEnumerable<CodeValuePair> presetArgs, Func<string, string> getValue=null) {
            string action = this.Title;
            string tool = this.ToolTemplate;
            string args = this.ToolArgsTemplate;
            try {
                StringBuilder sout = new StringBuilder(1024), serr = new StringBuilder(512);
                Func<string, string> synthGetVal;
                if (getValue == null) {
                    synthGetVal = this.GetVal;
                }
                else {
                    synthGetVal = (s) => { return getValue(s) ?? this.GetVal(s); };
                }
                TextStencil tt = new TextStencil() { FindParamValue = synthGetVal };
                if (presetArgs!=null) {
                    foreach(var p in presetArgs) {
                        tt.SetParameter(p.Code, p.Value);
                    }
                }

                tool = tt.Transform(this.ToolTemplate);
                args = tt.Transform(this.ToolArgsTemplate);

                action = string.Format("execute external cmd: \"{0}\" {1}", tool, args);
                var pcs= OSCommand.RunAndWait(tool, args, (s) => sout.AppendLine(s), (s) => serr.AppendLine(s), this.TimeoutSec*1000);
                System.Threading.Thread.Sleep(20); // Sometimes sout is not populated to the end, need some cycles
                System.Diagnostics.Debug.WriteLine("XCmd: {0}\r\n\tExit code: {1}\r\n\tDuration: {4}\r\n\tStdOut:----\r\n{5}\r\n\t----\r\n\tStdErr----{6}\r\n\t----\r\n", 
                    action, pcs.ExitCode, pcs.StartTime, pcs.ExitTime, (pcs.ExitTime - pcs.StartTime).TotalSeconds, sout, serr);
                if (pcs.ExitCode!=0) {
                    throw new BizException(string.Format("XCmd: {0}\r\n\tExit code: {1}\r\n\tDuration: {4}\r\n\tStdOut:----\r\n{5}\r\n\t----\r\n\tStdErr----{6}\r\n\t----\r\n",
                    action, pcs.ExitCode, pcs.StartTime, pcs.ExitTime, (pcs.ExitTime - pcs.StartTime).TotalSeconds, sout, serr));
                }
            }
            catch (BizException) {
                throw;
            }
            catch (Exception x) {
                x.Data["action"] = action;
                x.Data["xcmd"] = this.Code;
                AppContext.Current.LogTechError(string.Format("Failed to {0}", action), x);
                throw new BizException(x.ToShortMsg(action));
            }
        }

    }

    public class TemplParams {
        public static string DirPath = "dirPath";
        public static string FilePath = "filePath";
        public static string FileName = "fileName";
        public static string FileExt = "fileExt";
        public static string TrgFileName = "trgFileName";
        public static string TrgDirPath = "trgDirPath";
        public static string TimeStart = "tStart";
        public static string TimeStop = "tStop";
        public static string TimeDuration = "tDuration";

        /// <summary>
        /// Get file extension without dot
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileExt(string filePath) {
            return Path.GetExtension(filePath).Substring(1);
        }

        public static string GetDirPath(string filePath) {
            return Path.GetDirectoryName(filePath);
        }

        public static string GetFileName(string filePath) {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        public static IEnumerable<CodeValuePair> EnumerateStdSourceParams(ISourceRef source) {
            if ((source != null) && !string.IsNullOrEmpty(source.SourcePath)) {
                string tmp = source.SourcePath;

                yield return new CodeValuePair() { Code = TemplParams.FilePath, Value = tmp };
                yield return new CodeValuePair() { Code = TemplParams.DirPath, Value = TemplParams.GetDirPath(tmp) };
                yield return new CodeValuePair() { Code = TemplParams.FileName, Value = TemplParams.GetFileName(tmp) };
                yield return new CodeValuePair() { Code = TemplParams.FileExt, Value = TemplParams.GetFileExt(tmp) };
            }
        }

    }

}

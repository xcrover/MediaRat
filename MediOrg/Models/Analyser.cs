using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ops.NetCoe.LightFrame;

namespace MediOrg.Models {
    public class Analyser {
        public string CurrentFolder { get; protected set; }
        public Dictionary<string, FileTypeDsc> FTypes { get; protected set; }
        public Dictionary<string, FileNameTransDsc> FNames { get; protected set; }
        public List<FileDsc> Files { get; protected set; }
        public List<FileGroupDsc> FileGroups { get; protected set; }

        public Analyser(string currentFolder, IEnumerable<string> files) {
            this.CurrentFolder = currentFolder;
            FTypes= new Dictionary<string, FileTypeDsc>(StringComparer.CurrentCultureIgnoreCase);
            FNames = new Dictionary<string, FileNameTransDsc>(StringComparer.CurrentCultureIgnoreCase);
            Files = new List<FileDsc>();
            if (files!=null) {
                foreach(var f in files) {
                    RegisterType(f);
                    RegisterFile(f);
                    //RegisterNm(f);
                }
                CalcStats();
            }
        }

        void RegisterType(string fileName) {
            FileTypeDsc tmp;
            var fext = Path.GetExtension(fileName).ToLower();
            if (!this.FTypes.TryGetValue(fext, out tmp)) {
                tmp = new FileTypeDsc() {
                    FileExt = fext,
                    IsMarked= true,
                    Count=1
                };
                this.FTypes[fext] = tmp;
            }
            else {
                tmp.Count += 1;
            }
        }

        void RegisterNm(string fileName) {
            

        }

        void RegisterFile(string file) {
            string filePath = null;
            try {
                filePath = Path.Combine(this.CurrentFolder, file);
                var fd = new FileDsc() {
                    Name = file,
                    Title= Path.GetFileNameWithoutExtension(file),
                    Extension= Path.GetExtension(file).ToLower(),
                    FileTime = File.GetLastWriteTime(filePath)
                };
                this.Files.Add(fd);
            }
            catch(Exception x) {
                x.Data["file"] = filePath ?? file ?? "n/a";
            }
        }

        public List<DateTime> GetGroupTimes(double intervalM) {
            List<DateTime> rz = new List<DateTime>();
            DateTime dt= DateTime.MinValue;
            if (this.Files.Count > 0) {
                var q = from t in this.Files orderby t.FileTime select t.FileTime;
                foreach(var t in q) {
                    if (rz.Count==0) {
                        dt = t;
                        rz.Add(dt);
                    }
                    else {
                        if (dt.AddMinutes(intervalM)<t) {
                            dt = t;
                            rz.Add(dt);
                        }
                        else {
                            dt = t;
                        }
                    }
                }
            }
            return rz;
        }

        public List<FileGroupDsc> RecalcGroups(double intervalM, Func<int, string> grpNameGenerator=null) {
            List<FileGroupDsc> rz = new List<FileGroupDsc>();
            DateTime dt= DateTime.MinValue;
            Func<int, string> namer= grpNameGenerator ?? ((n)=>string.Format("Group #{0}", n));
            FileGroupDsc tmp = null;
            int ix = 1;
            if (this.Files.Count > 0) {
                var q = from t in this.Files orderby t.FileTime select t;
                foreach (var t in q) {
                    if (tmp == null) {
                        tmp = new FileGroupDsc() {
                            GrpName = namer(ix++),
                            StartTime = new DateTimeRef(t.FileTime),
                            EndTime = new DateTimeRef(DateTime.MaxValue),
                            Count=1
                        };
                        dt = t.FileTime;
                        t.Group = tmp;
                        rz.Add(tmp);
                    }
                    else {
                        if (dt.AddMinutes(intervalM) < t.FileTime) {
                            tmp.EndTime.Value = dt;
                            tmp = new FileGroupDsc() {
                                GrpName = namer(ix++),
                                StartTime = new DateTimeRef(t.FileTime),
                                EndTime = new DateTimeRef(DateTime.MaxValue),
                                Count = 1
                            };
                            dt = t.FileTime;
                            t.Group = tmp;
                            rz.Add(tmp);
                        }
                        else {
                            tmp.Count += 1;
                            t.Group = tmp;
                            dt = t.FileTime;
                        }
                    }
                }
                tmp.EndTime.Value = dt;
            }

            return this.FileGroups=rz;
        }

        void CalcStats() {
            foreach(var t in FTypes.Values) {
                t.Title = string.Format("{0} [{1}]", t.FileExt, t.Count);
            }
        }

        public override string ToString() {
            return string.Format("Found {0} files of {1} types", this.Files.Count, this.FTypes.Count);
        }
    }
}

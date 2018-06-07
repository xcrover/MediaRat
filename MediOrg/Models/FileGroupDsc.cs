using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ops.NetCoe.LightFrame;

namespace MediOrg.Models {
    public class FileGroupDsc : NotifyPropertyChangedBase {
        ///<summary>Group name</summary>
        private string _grpName;
        ///<summary>Start time</summary>
        private DateTimeRef _startTime;
        ///<summary>End time</summary>
        private DateTimeRef _endTime;
        ///<summary>Count</summary>
        private int _count;
        ///<summary>Is Accented</summary>
        private bool _isAccented;

        ///<summary>Is Accented</summary>
        public bool IsAccented {
            get { return this._isAccented; }
            set {
                if (this._isAccented != value) {
                    this._isAccented = value;
                    this.FirePropertyChanged(nameof(IsAccented));
                }
            }
        }


        ///<summary>Count</summary>
        public int Count {
            get { return this._count; }
            set {
                if (this._count != value) {
                    this._count = value;
                    this.FirePropertyChanged(nameof(Count));
                }
            }
        }


        public FileGroupDsc() {}

        public FileGroupDsc(DateTime startTime, DateTime endTime) {
            this._startTime = new DateTimeRef(startTime);
            this._endTime = new DateTimeRef(endTime);
        }

        ///<summary>End time</summary>
        public DateTimeRef EndTime {
            get { return this._endTime; }
            set {
                if (this._endTime != value) {
                    this._endTime = value;
                    this.FirePropertyChanged(nameof(EndTime));
                }
            }
        }


        ///<summary>Start time</summary>
        public DateTimeRef StartTime {
            get { return this._startTime; }
            set {
                if (this._startTime != value) {
                    this._startTime = value;
                    this.FirePropertyChanged(nameof(StartTime));
                }
            }
        }



        ///<summary>Group name</summary>
        public string GrpName {
            get { return this._grpName; }
            set {
                if (this._grpName != value) {
                    this._grpName = value;
                    this.FirePropertyChanged(nameof(GrpName));
                }
            }
        }

        public FileGroupDsc CreateNextGroup(string grpName, DateTime endTime) {
            var rz = new FileGroupDsc() {
                GrpName = grpName,
                StartTime = this.EndTime,
                EndTime = new DateTimeRef(endTime)
            };
            return rz;
        }

        /// <summary>
        /// Converts collection of DateTimes into groups ordered by time.
        /// </summary>
        /// <param name="times">Collection of DateTimes. May contain duplicates and may not be ordered.</param>
        /// <param name="grpNameGenerator">Group name generator. If <c>null</c> then default generator is used.</param>
        /// <returns></returns>
        public static IEnumerable<FileGroupDsc> GenerateSequence(IEnumerable<DateTime> times, Func<int, string> grpNameGenerator=null) {
            HashSet<DateTime> dths = new HashSet<DateTime>(times);
            var tml = (from t in dths orderby t select t).ToArray();
            Func<int, string> namer= grpNameGenerator??((x) => string.Format("Group #{0}", x));
            if (tml.Length>0) {
                if (tml.Length==1) {
                    yield return new FileGroupDsc() {
                        GrpName = namer(1),
                        StartTime = new DateTimeRef(tml[0]),
                        EndTime = new DateTimeRef(DateTime.MaxValue)
                    };
                }
                else {
                    FileGroupDsc fg = new FileGroupDsc() {
                        GrpName = namer(1),
                        StartTime = new DateTimeRef(tml[0]),
                        EndTime = new DateTimeRef(tml[1])
                    };
                    yield return fg;
                    for(int i=2;i<tml.Length;i++) {
                        fg= fg.CreateNextGroup(namer(i), tml[i]);
                        yield return fg;
                    }
                    fg = fg.CreateNextGroup(namer(tml.Length), DateTime.MaxValue);
                    yield return fg;
                }
            }
        }
    }
}

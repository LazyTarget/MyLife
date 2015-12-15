using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ProcessLib.Interfaces;

namespace ProcessLib.Models
{
    public class Process : IProcess, IValidatableObject
    {
        [Key]
        public long ID { get; set; }
        
        public int ProcessID { get; set;  }
        public string ProcessName { get; set; }
        public string MachineName { get; set; }
        public string ModuleName { get; set; }
        public string FileName { get; set; }
        
        //public string MainWindowTitle { get; set; }
        public virtual IList<ProcessTitle> Titles { get; set; }

        //IList<IProcessTitle> IProcess.Titles
        //{
        //    get { return Titles?.Cast<IProcessTitle>().ToList(); }
        //    set { Titles = value; }
        //}

        public bool HasExited { get; set; }
        public int? ExitCode { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public DateTime TimeAdded { get; set; }
        public DateTime TimeUpdated { get; set; }


        public void CopyFrom(IProcess process)
        {
            if (process == null)
                return;
            ID = process.ID;
            ProcessID = process.ProcessID;
            ProcessName = process.ProcessName;
            MachineName = process.MachineName;
            ModuleName = process.ModuleName;
            FileName = process.FileName;
            HasExited = process.HasExited;
            ExitCode = process.ExitCode;
            StartTime = process.StartTime;
            ExitTime = process.ExitTime;
            TimeAdded = process.TimeAdded;
            TimeUpdated = process.TimeUpdated;

            Titles = Titles ?? new List<ProcessTitle>();
            if (process.Titles != null)
            {
                //var list = new List<ProcessTitle>();
                //var typed = process.Titles.Where(x => x is ProcessTitle).Cast<ProcessTitle>();
                //list.AddRange(typed);

                //var nontyped = process.Titles.Where(x => !(x is ProcessTitle)).ToList();
                //nontyped.ForEach(x =>
                //{
                //    var t = new ProcessTitle();
                //    t.CopyFrom(x);
                //    list.Add(t);
                //});
                //Titles = list;

                foreach (var processTitle in process.Titles)
                {
                    if (processTitle.ID == 0 || Titles.All(y => y.ID != processTitle.ID))
                    {
                        var t = Titles.ToList();
                        Titles = new List<ProcessTitle>(Titles.Count + 4);
                        t.ForEach(Titles.Add);
                        Titles.Add(processTitle);
                    }
                    else
                    {
                        
                    }
                }
            }
            Titles = Titles.OrderBy(x => x.StartTime).ThenBy(x => x.ID).ToList();
        }

        public override string ToString()
        {
            return string.Format("#{0} {1}", ProcessID, ProcessName);
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartTime.HasValue)
            {
                if (StartTime == DateTime.MinValue)
                    yield return new ValidationResult("Invalid StartTime", new[] {nameof(StartTime)});
                if (StartTime == DateTime.MaxValue)
                    yield return new ValidationResult("Invalid StartTime", new[] {nameof(StartTime)});
            }
            if (ExitTime.HasValue)
            {
                if (ExitTime == DateTime.MinValue)
                    yield return new ValidationResult("Invalid EndTime", new[] { nameof(ExitTime) });
                if (ExitTime == DateTime.MaxValue)
                    yield return new ValidationResult("Invalid EndTime", new[] { nameof(ExitTime) });
            }
            //if (TimeAdded == DateTime.MinValue)
            //    yield return new ValidationResult("Invalid TimeAdded", new[] { nameof(TimeAdded) });
            //if (TimeUpdated == DateTime.MinValue)
            //    yield return new ValidationResult("Invalid TimeUpdated", new[] { nameof(TimeUpdated) });
        }
    }
}
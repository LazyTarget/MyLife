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


        //public override string ToString()
        //{
        //    return string.Format("#{0} {1}", ProcessID, ProcessName);
        //}

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

        public void Dispose()
        {
            
        }
    }
}
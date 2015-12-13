using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProcessLib.Interfaces;

namespace ProcessLib.Models
{
    public class ProcessTitle : IProcessTitle, IValidatableObject
    {
        [Key]
        public long ID { get; set; }
        public long ProcessID { get; set; }
        public virtual Process Process { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ProcessID <= 0)
                yield return new ValidationResult("Invalid ProcessID", new[] {nameof(ProcessID)});
            if (!StartTime.HasValue)
                yield return new ValidationResult("Invalid StartTime", new[] {nameof(StartTime)});
            if (EndTime.HasValue)
            {
                if (EndTime == DateTime.MinValue)
                    yield return new ValidationResult("Invalid EndTime", new[] { nameof(EndTime) });
                if (EndTime == DateTime.MaxValue)
                    yield return new ValidationResult("Invalid EndTime", new[] { nameof(EndTime) });
            }
        }
    }
}
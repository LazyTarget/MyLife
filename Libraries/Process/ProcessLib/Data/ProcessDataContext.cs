using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Threading;
using System.Threading.Tasks;
using ProcessLib.Models;

namespace ProcessLib.Data
{
    public class ProcessDataContext : DbContext
    {
        public ProcessDataContext()
        {
            
        }

        public IDbSet<Process> Processes { get; set; }
        public IDbSet<ProcessTitle> ProcessTitles { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Process>()
                .ToTable("Process_Processes")
                .HasOptional(x => x.Title)
                .WithRequired(x => x.Process)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ProcessTitle>()
                .ToTable("Process_Titles")
                .HasRequired(x => x.Process)
                .WithOptional(x => x.Title);


            base.OnModelCreating(modelBuilder);
        }



        public override int SaveChanges()
        {
            ValidateDates();
            var changes = base.SaveChanges();
            return changes;
        }

        public override Task<int> SaveChangesAsync()
        {
            ValidateDates();
            return base.SaveChangesAsync();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            ValidateDates();
            return base.SaveChangesAsync(cancellationToken);
        }


        private void ValidateDates()
        {
            foreach (var change in ChangeTracker.Entries())
            {
                if (change.State == EntityState.Deleted ||
                    change.State == EntityState.Unchanged)
                    continue;

                var values = change.CurrentValues;
                foreach (var name in values.PropertyNames)
                {
                    var value = values[name];
                    if (value is DateTime)
                    {
                        var date = (DateTime)value;
                        if (date < System.Data.SqlTypes.SqlDateTime.MinValue.Value)
                        {
                            values[name] = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
                        }
                        else if (date > System.Data.SqlTypes.SqlDateTime.MaxValue.Value)
                        {
                            values[name] = System.Data.SqlTypes.SqlDateTime.MaxValue.Value;
                        }
                    }
                }
            }
        }

    }
}

using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using MyLife.Data.Models;

namespace MyLife.Data.Context
{
    public class MyLifeDataContext : DbContext
    {
        public MyLifeDataContext()
        {
            
        }

        public virtual IDbSet<User> Users { get; set; }

        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
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

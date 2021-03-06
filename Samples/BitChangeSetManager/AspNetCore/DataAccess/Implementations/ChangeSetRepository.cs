﻿using BitChangeSetManager.DataAccess.Contracts;
using BitChangeSetManager.Model;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BitChangeSetManager.DataAccess.Implementations
{
    public class ChangeSetRepository : BitChangeSetManagerEfRepository<ChangeSet>, IChangeSetsRepository
    {
        protected override void SaveChanges()
        {
            SetCreatedOnValue();

            base.SaveChanges();
        }

        protected override async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SetCreatedOnValue();

            await base.SaveChangesAsync(cancellationToken);
        }

        private void SetCreatedOnValue()
        {
            DbContext.ChangeTracker.DetectChanges();

            foreach (DbEntityEntry<ChangeSet> changeSet in DbContext.ChangeTracker.Entries<ChangeSet>().Where(e => e.State == EntityState.Added))
            {
                changeSet.Entity.CreatedOn = DateTimeProvider.GetCurrentUtcDateTime();
            }
        }
    }
}
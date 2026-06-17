using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Models;
using LegalSystem.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.AccessControl;

namespace LegalSystem.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // keep this at top
            base.OnModelCreating(builder);
            
            builder.ConfigureWorkflow();
            
        }
        #region private functions
        //public override int SaveChanges()
        //{
        //    BeforeSaveChanges();
        //    return base.SaveChanges();
        //}
        //public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        //{
        //    BeforeSaveChanges();
        //    return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        //}


        private void BeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();

            foreach (var entry in ChangeTracker.Entries())
            {
                // AuditLogEntity Logging 
                if (entry.Entity is not IAuditLoggableFlag || entry.Entity is TblAuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                // implement audit logging
                var auditEntry = new AuditEntry(entry);
                auditEntry.TableName = entry.Entity.GetType().Name;
                auditEntries.Add(auditEntry);
                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }
                    object? replacedCurrentValue = null;
                    object? replacedOriginalValue = null;
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = AuditTypeEnum.Create;
                            auditEntry.NewValues[propertyName] = replacedCurrentValue ?? property.CurrentValue;
                            auditEntry.User = entry.Property("CreatedByName").CurrentValue != null ? entry.Property("CreatedByName").CurrentValue.ToString() : "Null";
                            break;

                        case EntityState.Deleted:
                            auditEntry.AuditType = AuditTypeEnum.Delete;
                            auditEntry.OldValues[propertyName] = replacedOriginalValue ?? property.OriginalValue;
                            auditEntry.User = entry.Property("UpdatedByName").CurrentValue != null ? entry.Property("UpdatedByName").CurrentValue.ToString() : "Null";
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                var isDeletableEntity = entry.Entity is IDeletableEntity;
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = isDeletableEntity && (bool)entry.Property("IsDeleted").CurrentValue == true ? AuditTypeEnum.SoftDelete : AuditTypeEnum.Update;
                                auditEntry.OldValues[propertyName] = replacedOriginalValue ?? property.OriginalValue;
                                auditEntry.NewValues[propertyName] = replacedCurrentValue ?? property.CurrentValue;
                                auditEntry.User = entry.Property("UpdatedByName").CurrentValue != null ? entry.Property("UpdatedByName").CurrentValue.ToString() : "Null";
                            }
                            break;
                    }
                }
            }
            foreach (var auditEntry in auditEntries)
            {
                TblAuditLogs.Add(auditEntry.ToAudit());
            }
        }

        #endregion

        // Audit Logs
        public DbSet<Reason> Reasons { get; set; }
        public DbSet<RefTaskType> RefTaskTypes { get; set; }
        public DbSet<TblAuditLog> TblAuditLogs { get; set; }

        // Ref
        public DbSet<RefCaseLevel> RefCaseLevels { get; set; }
        public DbSet<RefCaseStatus> RefCaseStatus { get; set; }
        public DbSet<RefCaseType> RefCaseTypes { get; set; }
        public DbSet<RefCourt> RefCourts { get; set; }
        public DbSet<RefReliefSought> RefReliefSoughts { get; set; }
        public DbSet<RefUserType> RefUserTypes { get; set; }
        public DbSet<RefTaskPriority> RefTaskPriorities { get; set; }
        public DbSet<RefTaskStatus> RefTaskStatus { get; set; }
        public DbSet<RefDocumentClassification> RefDocumentClassifications { get; set; }
        public DbSet<RefEventType> RefEventTypes { get; set; }

        // Tables
        public DbSet<TblUser> TblUsers { get; set; }
        public DbSet<TblCase> TblCases { get; set; }
        public DbSet<TblCaseTask> TblCaseTasks { get; set; }
        public DbSet<TblCaseDocument> TblCaseDocuments { get; set; }
        public DbSet<TblCaseTeam> TblCaseTeams { get; set; }
        public DbSet<TblCaseEvent> TblCaseEvent { get; set; }
        public DbSet<TblCaseNote> TblCaseNotes { get; set; }
        public DbSet<TblCaseAuditLog> TblCaseAuditLogs { get; set; }
        public DbSet<TblWorkFlow> TblWorkFlows { get; set; }
    }
}

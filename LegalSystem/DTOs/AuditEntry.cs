using LegalSystem.Enums;
using LegalSystem.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace LegalSystem.DTOs
{
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string? User { get; set; }
        public string? TableName { get; set; }
        public Dictionary<string, object>? KeyValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object>? OldValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object>? NewValues { get; } = new Dictionary<string, object>();
        public AuditTypeEnum? AuditType { get; set; }
        public List<string>? ChangedColumns { get; } = new List<string>();

        public TblAuditLog ToAudit()
        {
            var audit = new TblAuditLog();
            audit.UserId = User;
            audit.Type = AuditType.ToString();
            audit.TableName = TableName;
            audit.DateTime = DateTime.Now;
            audit.PrimaryKey = JsonSerializer.Serialize(KeyValues);
            audit.OldValues = OldValues is null || OldValues.Count == 0 ? "null" : JsonSerializer.Serialize(OldValues);
            audit.NewValues = NewValues is null || NewValues.Count == 0 ? "null" : JsonSerializer.Serialize(NewValues);
            audit.AffectedColumns = ChangedColumns is null || ChangedColumns.Count == 0 ? "null" : JsonSerializer.Serialize(ChangedColumns);
            return audit;
        }


        public class AuditCommand
        {
            public string? UserId { get; set; }
            public string? Type { get; set; }
            public string? TableName { get; set; }
            public DateTime DateTime { get; set; }
            public string? OldValues { get; set; }
            public string? NewValues { get; set; }
            public string? AffectedColumns { get; set; }
            public string? PrimaryKey { get; set; }
            public bool IsArchived { get; set; }
            public string? Comment { get; set; }
        }

    }
}

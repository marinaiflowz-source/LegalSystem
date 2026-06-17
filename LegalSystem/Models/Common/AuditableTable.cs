namespace LegalSystem.Models.Common
{
    public class AuditableTable
    {
        public DateTime CreatedOn { get; set; }
        public long? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedById { get; set; }
        public string? UpdatedByName { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace LegalSystem.DTOs
{
    public class CaseTeamCommand
    {
        public required long CaseId { get; set; }
        public required long UserId { get; set; }
    }

    public class CaseTeamQuery
    {
        public long Id { get; set; }
        public long CaseId { get; set; }
        public UserSummaryView? User { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedById { get; set; }
        public string? UpdatedByName { get; set; }
    }
}

using LegalSystem.Models.Common;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalSystem.Models
{
    public class TblCaseAuditLog
    {
        public Guid Id { get; set; }
        
        [Required]
        public long CaseId { get; set; }
        [ForeignKey(nameof(CaseId))]
        [InverseProperty(nameof(TblCase.AuditLogs))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public TblCase? Case { get; set; }
        public string? Comment { get; set; }

        public DateTime CreatedOn { get; set; }
        public long? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
    }
}

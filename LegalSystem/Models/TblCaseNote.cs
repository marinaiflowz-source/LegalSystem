using LegalSystem.Models.Common;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalSystem.Models
{
    public class TblCaseNote: AuditableTable, IAuditLoggableFlag
    {
        public long Id { get; set; }

        [Required]
        public long CaseId { get; set; }
        [ForeignKey(nameof(CaseId))]
        [InverseProperty(nameof(TblCase.Notes))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public TblCase? Case { get; set; }

        [Required]
        public string Note { get; set; } = string.Empty;
    }
}

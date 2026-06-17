using LegalSystem.Models.Common;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalSystem.Models
{
    public class TblCaseDocument: AuditableTable, IAuditLoggableFlag
    {
        public long Id { get; set; }

        [Required]
        public long CaseId { get; set; }
        [ForeignKey(nameof(CaseId))]
        [InverseProperty(nameof(TblCase.Documents))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public TblCase? Case { get; set; }


        [Required]
        public int ClassificationId { get; set; }

        [ForeignKey(nameof(ClassificationId))]
        [InverseProperty(nameof(RefDocumentClassification.Documents))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public RefDocumentClassification? Classification { get; set; }

        [Required]
        public string OriginalName { get; set; } = string.Empty;

        [Required]
        public string FileName { get; set; } = string.Empty;

        public string? Url { get; set; }
        public double SizeMB { get; set; }
    }
}

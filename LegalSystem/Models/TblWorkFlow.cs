using LegalSystem.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalSystem.Models
{
    public class TblWorkFlow : AuditableTable
    {
        [Key]
        [Column(Order = 0)] // Composite key
        public int CaseTypeId { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        public virtual RefCaseType? CaseType { get; set; }

        [Key]
        [Column(Order = 1)] // Composite key
        public int CaseStatusId { get; set; }

        [ForeignKey(nameof(CaseStatusId))]
        public RefCaseStatus? CaseStatus { get; set; }

        public int Order { get; set; }
    }
}

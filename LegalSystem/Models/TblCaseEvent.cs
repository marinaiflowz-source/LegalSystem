using LegalSystem.Models.Common;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalSystem.Models
{
    public class TblCaseEvent: AuditableTable, IAuditLoggableFlag
    {
        public long Id { get; set; }

        [Required]
        public long CaseId { get; set; }
        [ForeignKey(nameof(CaseId))]
        [InverseProperty(nameof(TblCase.Events))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public TblCase? Case { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        public int? CourtId { get; set; }

        [ForeignKey(nameof(CourtId))]
        [InverseProperty(nameof(RefCourt.Events))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public RefCourt? Court { get; set; }

        [Required]
        public int? TypeId { get; set; }

        [ForeignKey(nameof(TypeId))]
        [InverseProperty(nameof(RefEventType.Events))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public RefEventType? Type { get; set; }

        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}

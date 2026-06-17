using LegalSystem.Models.Common;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalSystem.Models
{
    public class TblCaseTeam: AuditableTable, IAuditLoggableFlag
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(TblUser.RelatedCases))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public TblUser? User { get; set; }

        public long CaseId { get; set; }
        [ForeignKey(nameof(CaseId))]
        [InverseProperty(nameof(TblCase.Team))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public TblCase? Case { get; set; }
    }
}

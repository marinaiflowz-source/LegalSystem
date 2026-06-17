using LegalSystem.Models.Common;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalSystem.Models
{
    public class TblUser: AuditableTable
    {
        public long Id { get; set; }
        public long RefId { get; set; }

        [Required]
        public string NameEn { get; set; } = string.Empty;

        public string NameAr { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public int TypeId { get; set; }

        [ForeignKey(nameof(TypeId))]
        [InverseProperty(nameof(RefUserType.Users))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public RefUserType? Type { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<TblCaseTeam> RelatedCases { get; set; } = new List<TblCaseTeam>();
        public virtual ICollection<TblCaseTask> RelatedTasks { get; set; } = new List<TblCaseTask>();
    }
}

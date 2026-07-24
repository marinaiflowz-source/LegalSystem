using LegalSystem.Models.Common;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalSystem.Models
{
    public class TblCaseTask: AuditableTable, IAuditLoggableFlag
    {
        public long Id { get; set; }

        [Required]
        public long CaseId { get; set; }
        [ForeignKey(nameof(CaseId))]
        [InverseProperty(nameof(TblCase.Tasks))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public TblCase? Case { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public int PriorityId { get; set; }
        public int? ReasonId { get; set; }
        public int? TaskTypeId { get; set; }
        [ForeignKey(nameof(TaskTypeId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public RefTaskType? RefTaskType { get; set; }

        [ForeignKey(nameof(PriorityId))]
        [InverseProperty(nameof(RefTaskPriority.Tasks))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public RefTaskPriority? Priority { get; set; }

        [Required]
        public int StatusId { get; set; }


        public bool IsClosed { get; set; } = false;

        [ForeignKey(nameof(StatusId))]
        [InverseProperty(nameof(RefTaskStatus.Tasks))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public RefTaskStatus? Status { get; set; }
        public string? Note { get; set; }

        [ForeignKey(nameof(ReasonId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Reason? Reason { get; set; }
        public long? AssignedUserId { get; set; }
        [ForeignKey(nameof(AssignedUserId))]
        [InverseProperty(nameof(TblUser.RelatedTasks))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public TblUser? AssignedUser { get; set; }
    }
}

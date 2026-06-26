using LegalSystem.Enums;
using LegalSystem.Models.Common;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalSystem.Models
{
    public class TblCase : AuditableTable, IAuditLoggableFlag
    {
        public long Id { get; set; }
        [Required]
        public string Claimant { get; set; } = string.Empty;

        [Required]
        public string Defendant { get; set; } = string.Empty;

        [Required]
        public int TypeId { get; set; }

        [ForeignKey(nameof(TypeId))]
        [InverseProperty(nameof(RefCaseType.Cases))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public RefCaseType? Type { get; set; }

        //[Required]
        //public int? LevelId { get; set; }

        //[ForeignKey(nameof(LevelId))]
        //[InverseProperty(nameof(RefCaseLevel.Cases))]
        //[DeleteBehavior(DeleteBehavior.Restrict)]
        //public RefCaseLevel? Level { get; set; }

        [Required]
        public int CourtId { get; set; }

        [ForeignKey(nameof(CourtId))]
        [InverseProperty(nameof(RefCourt.Cases))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public RefCourt? Court { get; set; }

        public decimal ClaimValue { get; set; }

        [Required]
        public int StatusId { get; set; }

        [ForeignKey(nameof(StatusId))]
        [InverseProperty(nameof(RefCaseStatus.Cases))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public RefCaseStatus? Status { get; set; }

        public string? Summary { get; set; }


        [Required]
        public int ReliefSoughtId { get; set; }

        [ForeignKey(nameof(ReliefSoughtId))]
        [InverseProperty(nameof(RefReliefSought.Cases))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public RefReliefSought? ReliefSought { get; set; }

        public long? ExpertWitnessId { get; set; }
        public string? ExpertWitnessNameEn { get; set; }
        public string? ExpertWitnessNameAr { get; set; }
        public string? UnitCode { get; set; }
        public bool IsCompleted { get; set; }

        public string CaseName { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public string? UnitNumber { get; set; }
        public string? UnitType { get; set; }
        public string? LeadStatus { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerNumber { get; set; }
        public string? JointBuyerName { get; set; }
        public string? JointBuyerMobile { get; set; }
        public string? SoldPrice { get; set; }

        public bool IsClaimant { get; set; }=false;
        public string? LeadID { get; set; }
        public CASEENUM? ClosedStatus { get; set; }
        public DateOnly? ClosedDate { get; set; }
        public long? MainCaseId { get; set; }

        [ForeignKey(nameof(MainCaseId))]
        public TblCase? MainCase { get; set; }
        public virtual ICollection<TblCaseTeam> Team { get; set; } = new List<TblCaseTeam>();
        public virtual ICollection<TblCaseTask> Tasks { get; set; } = new List<TblCaseTask>();
        public virtual ICollection<TblCaseDocument> Documents { get; set; } = new List<TblCaseDocument>();
        public virtual ICollection<TblCaseEvent> Events { get; set; } = new List<TblCaseEvent>();
        public virtual ICollection<TblCaseNote> Notes { get; set; } = new List<TblCaseNote>();
        public virtual ICollection<TblCaseAuditLog> AuditLogs { get; set; } = new List<TblCaseAuditLog>();
    }
}

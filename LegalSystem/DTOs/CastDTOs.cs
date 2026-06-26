using LegalSystem.Enums;
using LegalSystem.Models.Common;

namespace LegalSystem.DTOs
{
    public class CaseCommand
    {
        public required string Claimant { get; set; }
        public required string Defendant { get; set; }
        //public required int StatusId { get; set; }
        public required int TypeId { get; set; }
        //public required int LevelId { get; set; }
        public required int StatusId { get; set; }
        public required int CourtId { get; set; }
        public required decimal ClaimValue { get; set; }
        public string? Summary { get; set; }
        public required int ReliefSoughtId { get; set; }
        public long? ExpertWitnessId { get; set; }
        public string? ExpertWitnessNameEn { get; set; }
        public string? ExpertWitnessNameAr { get; set; }
        public string? UnitCode { get; set; }
        public string CaseName { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public string? UnitNumber { get; set; }
        public string?  LeadID { get; set; }

        
        public string? UnitType { get; set; }
        public string? LeadStatus { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerNumber { get; set; }
        public string? JointBuyerName { get; set; }
        public string? JointBuyerMobile { get; set; }
        public string? SoldPrice { get; set; }
        public bool IsClaimant { get; set; }
        public long? MainCaseId { get; set; }
    }
    public class UpdateCaseCommand
    {
        public string? Claimant { get; set; }
        public string? Defendant { get; set; }
        //public int? StatusId { get; set; }
        public int? TypeId { get; set; }
        //public int? LevelId { get; set; }
        public int? CourtId { get; set; }
        public decimal? ClaimValue { get; set; }
        public string? Summary { get; set; }
        public int? ReliefSoughtId { get; set; }
        public long? ExpertWitnessId { get; set; }
        public string? ExpertWitnessNameEn { get; set; }
        public string? ExpertWitnessNameAr { get; set; }
        public string? UnitCode { get; set; }
        public string CaseName { get; set; }
        public bool IsClaimant { get; set; }
        public long? MainCaseId { get; set; }
    }
    public class CloseCaseCommand
    {
        public int ClosedStatus { get; set; }
        public bool IsCompleted { get; set; }
        public DateOnly ClosedDate { get; set; }
    }
    public class CaseQuery
    {
        public long Id { get; set; }
        public string Claimant { get; set; } = string.Empty;
        public string Defendant { get; set; } = string.Empty;
        public SummaryView Type { get; set; } = new SummaryView();
        public SummaryView Level { get; set; } = new SummaryView();
        public SummaryView Court { get; set; } = new SummaryView();
        public decimal ClaimValue { get; set; }
        public SummaryView Status { get; set; } = new SummaryView();
        public SummaryView? NextStatus { get; set; }
        public string? Summary { get; set; }
        public SummaryView ReliefSought { get; set; } = new SummaryView();
        //public SummaryView? ExpertWitness { get; set; }
        public string? ExpertWitness { get; set; }
        public string? UnitCode { get; set; }
        public bool IsCompleted { get; set; }

        public DateTime CreatedOn { get; set; }
        public long? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedById { get; set; }
        public string? UpdatedByName { get; set; }
        public string? CaseName { get; set; }

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

        public string? LeadID { get; set; }
        public DateOnly? ClosedDate { get; set; }
        public CASEENUM? ClosedStatus { get; set; }
        public  bool IsClaimant { get; set; }
        public SummaryView? MainCase { get; set; }
    }
    public class CaseDetailsQuery : CaseQuery
    {
        public IReadOnlyList<CaseNoteView> Notes { get; set; } = new List<CaseNoteView>();
    }
    public class CaseAuditLogQuery
    {
        public Guid Id { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
    }
    public class CaseNoteView
    {
        public long Id { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
    }
    public class CaseFilter: IQueryObject
    {
        public string? SortBy { get; set; }
        public bool IsAscending { get; set; }
        public int Index { get; set; }
        public int Size { get; set; }

        public string? SearchText { get; set; }
        public int? TypeId { get; set; }
        public int? LevelId { get; set; }
        public int? CourtId { get; set; }
        public int? StatusId { get; set; }
        public int? ReliefSoughtId { get; set; }
        public string? LeadID { get; set; }
        public long? MainCaseId { get; set; }
    }


    public class SubCaseFilter : IQueryObject
    {
        public string? SortBy { get; set; }
        public bool IsAscending { get; set; }
        public int Index { get; set; }
        public int Size { get; set; }

        public long? MainCaseId { get; set; }
      
    }

    public class CaseList
    {
        public long Id { get; set; }
        public string CaseName { get; set; } = string.Empty;
    }

    }

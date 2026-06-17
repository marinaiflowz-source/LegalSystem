using LegalSystem.Enums;

namespace LegalSystem.DTOs
{
    public class BaseRefCommand
    {
        public required string NameEN { get; set; }
        public required string NameAR { get; set; }
        public int? Order { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateBaseRefCommand
    {
        public string? NameEN { get; set; }
        public string? NameAR { get; set; }
        public int? Order { get; set; }
        public bool? IsActive { get; set; }
    }


    public class UpdateReasonBaseRefCommand
    {
        public string? NameEN { get; set; }
        public string? NameAR { get; set; }
        public int? Order { get; set; }
        public bool? IsActive { get; set; }
        public ReasonTypeEnum? ReasonType { get; set; }
    }
    public class RefefancesQuery
    {
        public IReadOnlyList<BaseRefQuery> CaseTypes { get; set; } = new List<BaseRefQuery>();
        public IReadOnlyList<BaseRefQuery> CaseLevels { get; set; } = new List<BaseRefQuery>();
        public IReadOnlyList<BaseRefQuery> CaseStatuses { get; set; } = new List<BaseRefQuery>();
        public IReadOnlyList<BaseRefQuery> Courts { get; set; } = new List<BaseRefQuery>();
        public IReadOnlyList<BaseRefQuery> ReliefSoughts { get; set; } = new List<BaseRefQuery>();
        public IReadOnlyList<BaseRefQuery> UserTypes { get; set; } = new List<BaseRefQuery>();
        public IReadOnlyList<BaseRefQuery> TaskPriorities { get; set; } = new List<BaseRefQuery>();
        public IReadOnlyList<BaseRefQuery> TaskStatuses { get; set; } = new List<BaseRefQuery>();
        public IReadOnlyList<BaseRefQuery> DocumentClassifications { get; set; } = new List<BaseRefQuery>();
        public IReadOnlyList<BaseRefQuery> EventTypes { get; set; } = new List<BaseRefQuery>();
        public IReadOnlyList<BaseRefQuery> Reasons { get; set; } = new List<BaseRefQuery>();
        public IReadOnlyList<BaseRefQuery> TaskTypes { get; set; } = new List<BaseRefQuery>();
    }

    public class BaseRefQuery
    {
        public int Id { get; set; }
        public string NameEN { get; set; } = string.Empty;
        public string NameAR { get; set; } = string.Empty;
        public int? Order { get; set; }
        public bool IsActive { get; set; }
    }
    public class BaseReasonRefQuery
    {
        public int Id { get; set; }
        public string NameEN { get; set; } = string.Empty;
        public string NameAR { get; set; } = string.Empty;
        public int? Order { get; set; }
        public bool IsActive { get; set; }

        public ReasonTypeEnum ReasonType { get; set; }
    }
    public class BaseRefFilter
    {
        public string? SearchText { get; set; }
        public bool? IsActive { get; set; }
    }
}

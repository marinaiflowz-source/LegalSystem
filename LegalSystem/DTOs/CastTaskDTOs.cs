namespace LegalSystem.DTOs
{
    public class CaseTaskCommand
    {
        public required long CaseId { get; set; }
        public required string Title { get; set; }
        public required DateTime DueDate { get; set; }
        public int PriorityId { get; set; }
        //public required int StatusId { get; set; }
        public long? AssignedUserId { get; set; }
        public int TaskTypeId { get; set; }
        public string? Note { get; set; }
    }

    public class CaseTaskUpdate
    {
        public string? Title { get; set; }
        public DateTime? DueDate { get; set; }
        public int? PriorityId { get; set; }
        
        public long? AssignedUserId { get; set; }
        public int? TaskTypeId { get; set; }
        public string? Note { get; set; }
    }

    public class CaseTaskClose
    {
      
        public int? ReasonId { get; set; }
        
    }

    public class CaseTaskQuery
    {
        public long Id { get; set; }
        public long CaseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public SummaryView? Priority { get; set; }
        public SummaryView? Status { get; set; }
        public SummaryView? Court { get; set; }

        public UserSummaryView? User { get; set; }


        public DateTime CreatedOn { get; set; }
        public long? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedById { get; set; }
        public string? UpdatedByName { get; set; }
        public bool IsClosed { get; set; }

        public ReasonView? Reason { get; set; }
        public RefTaskTypeView TaskType { get; set; }
        public string? Note { get; set; }
        
    }
    public class CaseTaskFilter : IQueryObject
    {
        public string? SortBy { get; set; }
        public bool IsAscending { get; set; }
        public int Index { get; set; }
        public int Size { get; set; }

        public string? SearchText { get; set; }
        public int? StatusId { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
    }
}

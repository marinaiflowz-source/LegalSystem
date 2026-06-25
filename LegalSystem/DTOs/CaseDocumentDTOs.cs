namespace LegalSystem.DTOs
{
    public class CaseDocumentCommand
    {
        public required long CaseId { get; set; }
        public required int ClassificationId { get; set; }
        public required IFormFile File { get; set; }
        public string DocumentName { get; set; }
    }

    public class CaseDocumentUpdate
    {
        //public int? ClassificationId { get; set; }
        public string? OriginalName { get; set; }
        public string? DocumentName { get; set; }
    }

    public class CaseDocumentQuery
    {
        public long Id { get; set; }

        public long CaseId { get; set; }

        public SummaryView? Classification { get; set; }

        public string OriginalName { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string? Url { get; set; }

        public double SizeMB { get; set; }

        public DateTime CreatedOn { get; set; }
        public long? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public string DocumentName { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedById { get; set; }
        public string? UpdatedByName { get; set; }
        public string CaseName { get; set; }
        public string CaseNumber { get; set; }
    }

    public class CaseDocumentFilter : IQueryObject
    {
        public string? SortBy { get; set; }
        public bool IsAscending { get; set; }
        public int Index { get; set; }
        public int Size { get; set; }

        public string? SearchText { get; set; }
        public int? ClassificationId { get; set; }
    }
}

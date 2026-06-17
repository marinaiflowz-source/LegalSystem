namespace LegalSystem.DTOs
{
    public class CaseEventCommand
    {
        public required long CaseId { get; set; }

        public required string Title { get; set; }

        public required DateTime Date { get; set; }

        public int? CourtId { get; set; }

        public required int? TypeId { get; set; }

        public string? Address { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }

    public class CaseEventUpdate
    {
        public string? Title { get; set; }

        public DateTime? Date { get; set; }

        public int? CourtId { get; set; }

        public int? TypeId { get; set; }

        public string? Address { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }

    public class CaseEventQuery
    {
        public long Id { get; set; }

        public long CaseId { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public SummaryView? Court { get; set; }

        public SummaryView? Type { get; set; }

        public string? Address { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public DateTime CreatedOn { get; set; }

        public long? CreatedById { get; set; }

        public string? CreatedByName { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public long? UpdatedById { get; set; }

        public string? UpdatedByName { get; set; }
    }
    public class CaseEventFilter
    {
        public required DateTime DueDateFrom { get; set; }
        public required DateTime DueDateTo { get; set; }
    }
}

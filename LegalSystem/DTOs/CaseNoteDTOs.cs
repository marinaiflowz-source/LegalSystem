namespace LegalSystem.DTOs
{
    public class CaseNoteCommand
    {
        public required long CaseId { get; set; }

        public required string Note { get; set; }
    }

    public class CaseNoteUpdate
    {
        public string? Note { get; set; }
    }

    public class CaseNoteQuery
    {
        public long Id { get; set; }

        public long CaseId { get; set; }

        public string Note { get; set; } = string.Empty;
    }
}

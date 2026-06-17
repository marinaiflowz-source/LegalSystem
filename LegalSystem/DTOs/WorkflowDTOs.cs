namespace LegalSystem.DTOs
{
    public class WorkflowCommand
    {
        public required int CaseTypeId { get; set; }
        public required IReadOnlyList<WorkflowStepCommand> Steps { get; set; } = new List<WorkflowStepCommand>();
    }
    public class WorkflowStepCommand
    {
        public int StatusId { get; set; }
        public int Order { get; set; }
    }



    public class WorkflowQuery
    {
        public int CaseTypeId { get; set; }
        public string CaseTypeName { get; set; } = string.Empty;

        public IReadOnlyList<WorkflowStepQuery> Steps { get; set; } = new List<WorkflowStepQuery>();
    }
    public class WorkflowStepQuery
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}

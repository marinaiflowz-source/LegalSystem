namespace LegalSystem.DTOs
{
    public class DashboardDto
    {
    public  CaseStatisticsDto CaseStatistics { get; set; }
    public List<TeamPerformanceDto> TeamPerformance { get; set; }
    public List<LitigationDensityDto> LitigationDensity { get; set; }

    public  List<ProjectOutcomeDto> ProjectOutcome { get; set; }

    public List<ActiveWorkflowDto> ActiveWorkflow { get; set; }
    public List<MonthlyCaseStatisticsDto> MonthlyCaseStatistics { get; set; }
    public List<UserCaseStatisticsDto> UserCaseStatistics { get; set; }
        
    }


    public class CaseStatisticsDto
    {
        public int WonCount { get; set; }
        public int LostCount { get; set; }
        public int SettledCount { get; set; }
        public int OpenCases { get; set; }
        public int ClosedCases { get; set; }
        public int ClosedCasesToday { get; set; }
    }

    public class TeamPerformanceDto
    {
        public string UserName { get; set; }
        public int WonCases { get; set; }
        public int LostCases { get; set; }
        public int TotalClosedCases { get; set; }
        public decimal WinPercentage { get; set; }
    }

    public class LitigationDensityDto
    {
        public string UnitCode { get; set; }
        public string ProjectName { get; set; }
        public int CaseCount { get; set; }
    }


    public class ProjectOutcomeDto
    {
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public int WonCases { get; set; }
        public int TotalClosedCases { get; set; }
        public decimal SuccessRate { get; set; }
    }

    public class ActiveWorkflowDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public DateTime? DueDate { get; set; }
        public int PriorityId { get; set; }
        public int StatusId { get; set; }
    }

    public class MonthlyCaseStatisticsDto
    {
        public string Month { get; set; }
        public int ClosedTotal { get; set; }
        public int TotalFilings { get; set; }
        public int WonCases { get; set; }
        public int SettledCases { get; set; }
        public int LostCases { get; set; }
        
    }

    public class UserCaseStatisticsDto
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public int TotalCases { get; set; }

        public int OpenCases { get; set; }

        public int ClosedCases { get; set; }

        public int WonCases { get; set; }

        public int SettledCases { get; set; }

        public int LostCases { get; set; }

        public decimal WinPercentage { get; set; }
    }
}

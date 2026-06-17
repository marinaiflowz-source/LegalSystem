namespace LegalSystem.Models
{
    public class RefCaseType
    {
        public int Id { get; set; }
        public string NameEN { get; set; } = string.Empty;
        public string NameAR { get; set; } = string.Empty;
        public int? Order { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<TblCase>? Cases { get; set; }
        public virtual ICollection<TblWorkFlow> WorkflowSteps { get; set; } = new List<TblWorkFlow>();
    }
}

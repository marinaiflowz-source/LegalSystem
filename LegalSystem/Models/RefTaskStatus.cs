namespace LegalSystem.Models
{
    public class RefTaskStatus
    {
        public int Id { get; set; }
        public string NameEN { get; set; } = string.Empty;
        public string NameAR { get; set; } = string.Empty;
        public int? Order { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<TblCaseTask>? Tasks { get; set; }
    }
}

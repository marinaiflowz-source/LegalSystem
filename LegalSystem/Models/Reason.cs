using LegalSystem.Enums;

namespace LegalSystem.Models
{
    public class Reason
    {
        public int Id { get; set; }
        public string NameEN { get; set; } = string.Empty;
        public string NameAR { get; set; } = string.Empty;
        public int? Order { get; set; }
        public bool IsActive { get; set; }

        public ReasonTypeEnum ReasonType { get; set; }
    }
}

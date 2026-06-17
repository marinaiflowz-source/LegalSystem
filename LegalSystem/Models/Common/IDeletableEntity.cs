namespace LegalSystem.Models.Common
{
    public interface IDeletableEntity
    {
        bool IsDeleted { get; set; }
    }
}

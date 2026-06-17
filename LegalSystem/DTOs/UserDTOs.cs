using LegalSystem.Attributes;
using System.ComponentModel.DataAnnotations;

namespace LegalSystem.DTOs
{
    public class UserCommand
    {
        public required string NameEn { get; set; }
        public required string NameAr { get; set; }
        [EmailAddress]
        public required string Email { get; set; }

        [NoLeadingZero]
        public required string PhoneNumber { get; set; }
        public required int TypeId { get; set; }
        public int? RefId { get; set; }
        public int? ManagerRefId { get; set; }
        public required string Password { get; set; }
    }

    public class UpdateUserCommand
    {
        public string? NameEn { get; set; }
        public string? NameAr { get; set; }
        public long? RefId { get; set; }
        public int? TypeId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UserQuery
    {
        public long Id { get; set; }
        public long RefId { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public int TypeId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedById { get; set; }
        public string? UpdatedByName { get; set; }
    }

    public class UserFilter : IQueryObject
    {
        public string? SortBy { get; set; }
        public bool IsAscending { get; set; }
        public int Index { get; set; }
        public int Size { get; set; }

        public string? SearchText { get; set; }
        public int? TypeId { get; set; }
    }
}

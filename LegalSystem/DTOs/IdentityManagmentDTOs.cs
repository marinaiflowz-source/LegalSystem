using LegalSystem.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LegalSystem.DTOs
{
    public class AddIdentityUserCommand
    {
        public required string NameEn { get; set; }

        public required string NameAr { get; set; }

        [EmailAddress]
        public required string Email { get; set; }
        public int? CompanyId { get; set; } = 4;

        [NoLeadingZero]
        public required string PhoneNumber { get; set; }
        public string? CountryCode { get; set; } = "971";
        public int UserTypeId { get; set; }
        public required string Password { get; set; }
        public int? ManagerId { get; set; }
    }
    public class AddIdentityUserResponse
    {
        public int UserId { get; set; }
    }
}

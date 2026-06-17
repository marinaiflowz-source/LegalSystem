using System.ComponentModel.DataAnnotations;

namespace LegalSystem.Attributes
{
    public class NoLeadingZeroAttribute : ValidationAttribute
    {
        public NoLeadingZeroAttribute()
        {
            ErrorMessage = "InvalidMobileNo";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string mobileNumber)
            {
                if (mobileNumber.StartsWith("0"))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}

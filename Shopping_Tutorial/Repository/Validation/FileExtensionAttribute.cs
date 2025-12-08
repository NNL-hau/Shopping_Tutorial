using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Repository.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is FormFile file)
            {
                var extension = Path.GetExtension(file.FileName);
                string[] extensions = { "jpg", "png", "jpeg" };
                bool result = extensions.Any(x => extension.EndsWith(x));
                if (!result)
                {
                    return new ValidationResult("Allowed extensions are jpg,png or jpeg");
                }
            }
            return ValidationResult.Success;
        }
    }
}

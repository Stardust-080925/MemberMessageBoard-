// 這個自訂屬性是要用在選擇圖片給圖片限制
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;

    public AllowedExtensionsAttribute(string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var file = value as IFormFile;
        if (file != null)
        {
            var ext = Path.GetExtension(file.FileName)?.ToLower().Trim();
            if (!_extensions.Contains(ext))
            {
                return new ValidationResult(ErrorMessage ?? $"僅允許 {string.Join(", ", _extensions)} 格式");
            }
        }
        return ValidationResult.Success;
    }
}
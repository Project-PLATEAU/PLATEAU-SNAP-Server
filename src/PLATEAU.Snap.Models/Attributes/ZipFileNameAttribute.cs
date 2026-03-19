using System.ComponentModel.DataAnnotations;

namespace PLATEAU.Snap.Models.Attributes;

public class ZipFileNameAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var fileName = value as string;

        if (string.IsNullOrEmpty(fileName))
        {
            return ValidationResult.Success;
        }

        if (!fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return new ValidationResult("ファイル名は .zip 拡張子で終わっている必要があります。");
        }

        return ValidationResult.Success;
    }
}
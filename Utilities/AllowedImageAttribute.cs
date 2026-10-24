using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Utilities;

/// <summary>
/// Accepts only JPEG, PNG or WEBP uploads up to <see cref="MaxBytes"/>, checking both the
/// extension and the file signature so renamed files (e.g. .html as .jpg) are rejected.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class AllowedImageAttribute : ValidationAttribute
{
    public const long MaxBytes = 2 * 1024 * 1024;
    public const string AcceptedExtensions = ".jpg,.jpeg,.png,.webp";

    private static readonly HashSet<string> Extensions = new(AcceptedExtensions.Split(','), StringComparer.OrdinalIgnoreCase);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IFormFile file) return ValidationResult.Success;

        if (!Extensions.Contains(Path.GetExtension(file.FileName)))
            return new ValidationResult("Formato no permitido. Usá JPG, PNG o WEBP.");

        if (file.Length == 0 || file.Length > MaxBytes)
            return new ValidationResult("La imagen debe pesar menos de 2 MB.");

        if (!HasImageSignature(file))
            return new ValidationResult("El archivo no es una imagen válida.");

        return ValidationResult.Success;
    }

    private static bool HasImageSignature(IFormFile file)
    {
        Span<byte> header = stackalloc byte[12];
        using var stream = file.OpenReadStream();
        var read = stream.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);
        if (read < header.Length) return false;

        var isJpeg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        var isPng = header[..4].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        var isWebp = header[..4].SequenceEqual("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8);

        return isJpeg || isPng || isWebp;
    }
}

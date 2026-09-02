using System.ComponentModel.DataAnnotations;

namespace GuadalupeDigital.Application.DTOs.Requests;

public class CreatePresignedPostRequest
{
    [Required(ErrorMessage = "Filename is required.")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Filename contains invalid characters.")]
    public string Filename { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content type is required.")]
    public string ContentType { get; set; } = string.Empty;

    [Range(1, 10 * 1024 * 1024, ErrorMessage = "File size must be between 1 byte and 10 MB.")]
    public long FileSize { get; set; }
}

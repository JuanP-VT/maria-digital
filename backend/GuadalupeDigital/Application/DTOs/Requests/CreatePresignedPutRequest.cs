using System.ComponentModel.DataAnnotations;

namespace GuadalupeDigital.Application.DTOs.Requests;

public class CreatePresignedPutRequest
{
	[Required(ErrorMessage = "Filename is required.")]
	[RegularExpression(
		@"^[a-zA-Z0-9._-]+$",
		ErrorMessage = "Filename contains invalid characters."
	)]
	public string Filename { get; set; } = string.Empty;

	[Required(ErrorMessage = "Content type is required.")]
	public string ContentType { get; set; } = string.Empty;
}

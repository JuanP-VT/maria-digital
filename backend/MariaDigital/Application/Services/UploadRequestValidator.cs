using MariaDigital.Application.DTOs.Requests;
using MariaDigital.Domain.Students;

namespace MariaDigital.Application.Services;

public static class UploadRequestValidator
{
	public const int MaxFiles = 4;
	public const long MaxSizeBytes = 10 * 1024 * 1024;

	private static readonly HashSet<string> AllowedContentTypes =
	[
		"image/jpeg",
		"image/png",
		"application/pdf",
	];

	/// <summary>Case-insensitive step id. Numeric enum values are not accepted.</summary>
	public static bool TryParseType(string? value, out StudentDocumentType type)
	{
		type = default;
		return !string.IsNullOrWhiteSpace(value)
			&& !char.IsDigit(value.Trim()[0])
			&& Enum.TryParse(value.Trim(), ignoreCase: true, out type)
			&& Enum.IsDefined(type);
	}

	public static string NormalizeContentType(string? contentType) =>
		(contentType ?? string.Empty).Trim().ToLowerInvariant();

	/// <summary>Spanish error messages; empty when the request is acceptable.</summary>
	public static IReadOnlyList<string> Validate(PresignUploadsRequest? request)
	{
		var files = request?.Files;
		if (files is null || files.Count == 0)
			return ["Debes enviar al menos un archivo."];
		if (files.Count > MaxFiles)
			return [$"Máximo {MaxFiles} archivos por envío."];

		var errors = new List<string>();
		var seen = new HashSet<StudentDocumentType>();
		foreach (var file in files)
		{
			if (file is null || !TryParseType(file.Type, out var type))
			{
				errors.Add($"Tipo de documento no válido: {file?.Type}.");
				continue;
			}
			if (!seen.Add(type))
				errors.Add($"El documento '{type}' está repetido.");
			if (!AllowedContentTypes.Contains(NormalizeContentType(file.ContentType)))
				errors.Add($"Tipo de archivo no permitido para '{type}'. Usa JPG, PNG o PDF.");
			if (file.SizeBytes <= 0)
				errors.Add($"El archivo '{type}' está vacío.");
			else if (file.SizeBytes > MaxSizeBytes)
				errors.Add($"El archivo '{type}' supera {MaxSizeBytes / (1024 * 1024)} MB.");
		}
		return errors;
	}
}

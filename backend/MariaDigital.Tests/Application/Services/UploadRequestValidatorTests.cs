using MariaDigital.Application.DTOs.Requests;
using MariaDigital.Application.Services;
using MariaDigital.Domain.Students;

namespace MariaDigital.Tests.Application.Services;

public class UploadRequestValidatorTests
{
	private static PresignFileRequest File(
		string? type = "birth",
		string? contentType = "image/jpeg",
		long size = 1000
	) => new(type, contentType, size);

	private static PresignUploadsRequest Request(params PresignFileRequest[] files) => new(files);

	[Fact]
	public void Accepts_one_file_of_each_type()
	{
		var request = Request(
			File("registration"),
			File("birth", "image/png"),
			File("baptism", "application/pdf"),
			File("address")
		);

		Assert.Empty(UploadRequestValidator.Validate(request));
	}

	[Fact]
	public void Rejects_a_missing_or_empty_request()
	{
		Assert.NotEmpty(UploadRequestValidator.Validate(null));
		Assert.NotEmpty(UploadRequestValidator.Validate(new PresignUploadsRequest(null)));
		Assert.NotEmpty(UploadRequestValidator.Validate(Request()));
	}

	[Fact]
	public void Rejects_more_than_four_files()
	{
		var request = Request(
			File("registration"),
			File("birth"),
			File("baptism"),
			File("address"),
			File()
		);

		Assert.Single(UploadRequestValidator.Validate(request));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("passport")]
	[InlineData("0")]
	[InlineData("99")]
	public void Rejects_unknown_document_types_including_numeric_enum_values(string? type)
	{
		Assert.NotEmpty(UploadRequestValidator.Validate(Request(File(type))));
	}

	[Fact]
	public void Rejects_a_repeated_type()
	{
		var errors = UploadRequestValidator.Validate(Request(File("birth"), File("BIRTH")));

		Assert.Single(errors);
	}

	[Theory]
	[InlineData("text/html")]
	[InlineData("image/svg+xml")]
	[InlineData("application/octet-stream")]
	[InlineData("")]
	[InlineData(null)]
	public void Rejects_content_types_outside_the_allowlist(string? contentType)
	{
		Assert.NotEmpty(UploadRequestValidator.Validate(Request(File(contentType: contentType))));
	}

	[Fact]
	public void Content_type_matching_ignores_case_and_whitespace()
	{
		Assert.Empty(UploadRequestValidator.Validate(Request(File(contentType: " Image/JPEG "))));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-5)]
	[InlineData(UploadRequestValidator.MaxSizeBytes + 1)]
	public void Rejects_empty_negative_and_oversized_files(long size)
	{
		Assert.NotEmpty(UploadRequestValidator.Validate(Request(File(size: size))));
	}

	[Fact]
	public void Accepts_a_file_exactly_at_the_size_limit()
	{
		Assert.Empty(
			UploadRequestValidator.Validate(
				Request(File(size: UploadRequestValidator.MaxSizeBytes))
			)
		);
	}

	[Fact]
	public void TryParseType_is_case_insensitive()
	{
		Assert.True(UploadRequestValidator.TryParseType(" Baptism ", out var type));
		Assert.Equal(StudentDocumentType.Baptism, type);
	}
}

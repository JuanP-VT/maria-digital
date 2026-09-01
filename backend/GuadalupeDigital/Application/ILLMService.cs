namespace GuadalupeDigital.Application;

public interface ILLMService
{
	public Task<string> GetTextFromImageAsync(byte[] imageBytes);
}

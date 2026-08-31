using GuadalupeDigital.Application;
using OpenAI;
using OpenAI.Chat;
namespace GuadalupeDigital.Infrastructure.LLMs
{
    public class OpenAiLLMService : ILLMService
    {
        private readonly ChatClient _chatClient;

        public OpenAiLLMService(ChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<string> GetTextFromImageAsync(byte[] imageBytes)
        {
            var imagePart = ChatMessageContentPart.CreateImagePart(
            BinaryData.FromBytes(imageBytes), "image/png");
            var textPart = ChatMessageContentPart.CreateTextPart(
            "Extract all text from this image. Return only the detected text.");
            var completion = await _chatClient.CompleteChatAsync(
            [ChatMessage.CreateUserMessage(textPart, imagePart)]);

            return completion.Value.Content.FirstOrDefault()?.Text ?? string.Empty;
        }
    }
}

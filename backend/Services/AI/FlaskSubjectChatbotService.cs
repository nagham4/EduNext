using System.Net.Http.Json;
using backend.DTOs.AI;

namespace backend.Services.AI;

public class FlaskSubjectChatbotService : ISubjectChatbotService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public FlaskSubjectChatbotService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<SubjectChatbotResponseDto> SendAsync(SubjectChatbotRequestDto request, CancellationToken ct = default)
    {
        var subjectKey = NormalizeSubjectKey(request);
        var baseUrl =
            _configuration[$"AiChatbot:Subjects:{subjectKey}:BaseUrl"] ??
            _configuration["AiChatbot:BaseUrl"] ??
            "http://localhost:5001";
        var chatUrl = $"{baseUrl.TrimEnd('/')}/chat";

        using var response = await _httpClient.PostAsJsonAsync(
            chatUrl,
            new
            {
                message = request.Message,
                subject = subjectKey,
                image_data = request.ImageData,
                image_mime_type = request.ImageMimeType
            },
            ct
        );
        response.EnsureSuccessStatusCode();

        var chatbotResponse = await response.Content.ReadFromJsonAsync<SubjectChatbotResponseDto>(cancellationToken: ct);

        return chatbotResponse ?? new SubjectChatbotResponseDto
        {
            Reply = "لم يصل رد واضح من الشات بوت."
        };
    }

    private static string NormalizeSubjectKey(SubjectChatbotRequestDto request)
    {
        var key = request.SubjectKey.Trim().ToLowerInvariant();

        if (key is "math" or "english")
        {
            return key;
        }

        var title = request.SubjectTitle.Trim().ToLowerInvariant();

        if (title.Contains("رياض") || title.Contains("math"))
        {
            return "math";
        }

        if (title.Contains("انج") || title.Contains("إنج") || title.Contains("english"))
        {
            return "english";
        }

        return "default";
    }
}

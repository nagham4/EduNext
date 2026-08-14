using backend.DTOs.AI;

namespace backend.Services.AI;

public interface ISubjectChatbotService
{
    Task<SubjectChatbotResponseDto> SendAsync(SubjectChatbotRequestDto request, CancellationToken ct = default);
}

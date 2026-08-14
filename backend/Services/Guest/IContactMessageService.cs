using backend.DTOs.Guest;

namespace backend.Services.Guest;

public interface IContactMessageService
{
    Task<ContactMessageDto> CreateAsync(CreateContactMessageDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<ContactMessageDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

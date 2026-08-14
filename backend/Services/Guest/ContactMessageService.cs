using backend.Data.Generated;
using backend.DTOs.Guest;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Guest;

public class ContactMessageService : IContactMessageService
{
    private readonly Data.Generated.AppDbContext _context;

    public ContactMessageService(Data.Generated.AppDbContext context)
    {
        _context = context;
    }

    public async Task<ContactMessageDto> CreateAsync(CreateContactMessageDto dto, CancellationToken cancellationToken)
    {
        var contactMessage = new ContactMessage
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim(),
            Subject = string.IsNullOrWhiteSpace(dto.Subject) ? null : dto.Subject.Trim(),
            Message = dto.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.ContactMessages.Add(contactMessage);
        await _context.SaveChangesAsync(cancellationToken);

        return new ContactMessageDto
        {
            Id = contactMessage.Id,
            Name = contactMessage.Name,
            Email = contactMessage.Email,
            Subject = contactMessage.Subject,
            Message = contactMessage.Message,
            CreatedAt = contactMessage.CreatedAt
        };
    }

    public async Task<IReadOnlyList<ContactMessageDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.ContactMessages
            .AsNoTracking()
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new ContactMessageDto
            {
                Id = m.Id,
                Name = m.Name,
                Email = m.Email,
                Subject = m.Subject,
                Message = m.Message,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var message = await _context.ContactMessages
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (message is null) return false;

        _context.ContactMessages.Remove(message);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
using backend.Data.Generated;
using backend.DTOs.Guest;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Guest
{
    public class FaqService
    {
        private readonly AppDbContext _context;

        public FaqService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<FaqDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.faqs
                .AsNoTracking()
                .OrderByDescending(f => f.created_at)
                .Select(f => new FaqDto
                {
                    Question = f.question,
                    Answer = f.answer
                })
                .ToListAsync(cancellationToken);
        }
    }
}

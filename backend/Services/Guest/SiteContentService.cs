using backend.Data.Generated;
using backend.DTOs.Guest;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Guest
{
    public class SiteContentService
    {
        private readonly AppDbContext _context;

        public SiteContentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SiteContentDto> GetHomeContentAsync(CancellationToken cancellationToken)
        {
            var data = await _context.site_contents
                .AsNoTracking()
                .ToDictionaryAsync(
                    s => s.content_key,
                    s => s.content_value,
                    cancellationToken);

            return new SiteContentDto
            {
                HeroTitle = data.GetValueOrDefault("hero_title"),
                HeroSubtitle = data.GetValueOrDefault("hero_subtitle"),
                IntroVideoUrl = data.GetValueOrDefault("intro_video_url"),
                FeaturesText = data.GetValueOrDefault("features_text"),
                HomeDescription = data.GetValueOrDefault("home_description")
            };
        }
    }
}

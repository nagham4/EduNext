using backend.Data.Generated;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Guest;

public class ContactMessageSchemaInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public ContactMessageSchemaInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await context.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS public."ContactMessages" (
                    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                    "Name" character varying(255) NOT NULL,
                    "Email" character varying(255) NOT NULL,
                    "Subject" character varying(150),
                    "Message" text NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
                );

                ALTER TABLE public."ContactMessages"
                ADD COLUMN IF NOT EXISTS "Subject" character varying(150);
                """,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"Could not ensure ContactMessages schema exists: {ex.Message}"
            );
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

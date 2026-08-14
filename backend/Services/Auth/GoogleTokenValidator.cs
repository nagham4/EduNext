using Google.Apis.Auth;

namespace backend.Services.Auth;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly IConfiguration _config;

    public GoogleTokenValidator(IConfiguration config)
    {
        _config = config;
    }

    public async Task<GoogleUserInfo?> ValidateAsync(string idToken)
    {
        if (string.IsNullOrWhiteSpace(idToken))
            return null;

        var googleClientId = _config["Google:ClientId"];

        if (string.IsNullOrWhiteSpace(googleClientId))
        {
            Console.WriteLine("Google login error: Google:ClientId is missing in appsettings.json.");
            return null;
        }

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { googleClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (payload == null)
                return null;

            if (string.IsNullOrWhiteSpace(payload.Subject))
                return null;

            if (string.IsNullOrWhiteSpace(payload.Email))
                return null;

            if (payload.EmailVerified != true)
                return null;

            return new GoogleUserInfo
            {
                GoogleUserId = payload.Subject,
                Email = payload.Email.Trim().ToLower(),
                FullName = string.IsNullOrWhiteSpace(payload.Name)
                    ? payload.Email.Split('@')[0]
                    : payload.Name.Trim(),
                EmailVerified = payload.EmailVerified
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("Google login validation error: " + ex.Message);
            return null;
        }
    }
}
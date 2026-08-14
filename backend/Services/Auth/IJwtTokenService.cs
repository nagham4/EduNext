using backend.Models.Generated;

namespace backend.Services.Auth;

public interface IJwtTokenService
{
    string GenerateToken(user user);
}
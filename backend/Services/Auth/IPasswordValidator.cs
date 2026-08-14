namespace backend.Services.Auth;

public interface IPasswordValidator
{
    List<string> GetPasswordSuggestions(string password);
}
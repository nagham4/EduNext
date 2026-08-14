namespace backend.Services.Auth;

public class AuthResult<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = "";
    public T? Data { get; set; }
    public Dictionary<string, List<string>>? Errors { get; set; }
    public List<string>? PasswordSuggestions { get; set; }

    public static AuthResult<T> Ok(T data, string message = "تمت العملية بنجاح.")
    {
        return new AuthResult<T>
        {
            Success = true,
            StatusCode = 200,
            Message = message,
            Data = data
        };
    }

    public static AuthResult<T> Fail(
        int statusCode,
        string message,
        Dictionary<string, List<string>>? errors = null,
        List<string>? passwordSuggestions = null)
    {
        return new AuthResult<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors,
            PasswordSuggestions = passwordSuggestions
        };
    }
}
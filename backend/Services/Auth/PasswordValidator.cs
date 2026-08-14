using System.Text.RegularExpressions;

namespace backend.Services.Auth;

public class PasswordValidator : IPasswordValidator
{
    public List<string> GetPasswordSuggestions(string password)
    {
        var suggestions = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            suggestions.Add("كلمة المرور مطلوبة.");
            suggestions.Add("استخدمي كلمة مرور تحتوي على 8 أحرف على الأقل.");
            suggestions.Add("أضيفي حرفًا كبيرًا مثل A.");
            suggestions.Add("أضيفي حرفًا صغيرًا مثل a.");
            suggestions.Add("أضيفي رقمًا واحدًا على الأقل.");
            suggestions.Add("أضيفي رمزًا خاصًا مثل ! أو @ أو #.");
            return suggestions;
        }

        if (password.Length < 8)
            suggestions.Add("كلمة المرور يجب أن تحتوي على 8 أحرف على الأقل.");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            suggestions.Add("أضيفي حرفًا كبيرًا واحدًا على الأقل.");

        if (!Regex.IsMatch(password, @"[a-z]"))
            suggestions.Add("أضيفي حرفًا صغيرًا واحدًا على الأقل.");

        if (!Regex.IsMatch(password, @"\d"))
            suggestions.Add("أضيفي رقمًا واحدًا على الأقل.");

        if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>_\-+=~`]"))
            suggestions.Add("أضيفي رمزًا خاصًا مثل ! أو @ أو #.");

        if (password.Contains(' '))
            suggestions.Add("كلمة المرور لا يجب أن تحتوي على مسافات.");

        return suggestions;
    }
}
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;

namespace backend.Services.Guest;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendContactMessageAsync(
        string senderName,
        string senderEmail,
        string? subject,
        string message,
        CancellationToken cancellationToken
    )
    {
        EnsureConfigured();

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName, Encoding.UTF8),
            Subject = $"EduNext Contact: {NormalizeSubject(subject)}",
            Body = BuildBody(senderName, senderEmail, subject, message),
            IsBodyHtml = false,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };

        mailMessage.To.Add(new MailAddress(_settings.ContactInbox));
        mailMessage.ReplyToList.Add(new MailAddress(senderEmail, senderName, Encoding.UTF8));

        using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }

    public async Task SendPasswordResetOtpAsync(
        string recipientEmail,
        string recipientName,
        string otp,
        CancellationToken cancellationToken
    )
    {
        try
        {
            EnsureConfigured();

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName, Encoding.UTF8),
                Subject = "EduNext password reset code",
                Body = BuildPasswordResetBody(recipientName, otp),
                IsBodyHtml = false,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mailMessage.To.Add(new MailAddress(recipientEmail, recipientName, Encoding.UTF8));

            using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password)
            };

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        }
        catch (Exception ex) when (
            ex is SmtpException or
            InvalidOperationException or
            FormatException or
            ArgumentException
        )
        {
            throw new EmailDeliveryException("Password reset email delivery failed.", ex);
        }
    }

    private void EnsureConfigured()
    {
        if (
            string.IsNullOrWhiteSpace(_settings.Host) ||
            string.IsNullOrWhiteSpace(_settings.Username) ||
            string.IsNullOrWhiteSpace(_settings.Password) ||
            string.IsNullOrWhiteSpace(_settings.FromEmail) ||
            string.IsNullOrWhiteSpace(_settings.ContactInbox)
        )
        {
            throw new InvalidOperationException("Email SMTP settings are missing.");
        }
    }

    private static string NormalizeSubject(string? subject)
    {
        return string.IsNullOrWhiteSpace(subject) ? "New message" : subject.Trim();
    }

    private static string BuildBody(string senderName, string senderEmail, string? subject, string message)
    {
        var builder = new StringBuilder();
        builder.AppendLine("New contact message from EduNext website");
        builder.AppendLine();
        builder.AppendLine($"Name: {senderName}");
        builder.AppendLine($"Email: {senderEmail}");
        builder.AppendLine($"Subject: {NormalizeSubject(subject)}");
        builder.AppendLine();
        builder.AppendLine("Message:");
        builder.AppendLine(message);

        return builder.ToString();
    }

    private static string BuildPasswordResetBody(string recipientName, string otp)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Hello {recipientName},");
        builder.AppendLine();
        builder.AppendLine("Use this EduNext verification code to reset your password:");
        builder.AppendLine();
        builder.AppendLine(otp);
        builder.AppendLine();
        builder.AppendLine("This code is valid for 3 minutes. If you did not request a password reset, ignore this email.");

        return builder.ToString();
    }
}

public class EmailDeliveryException : Exception
{
    public EmailDeliveryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

namespace backend.Services.Guest;

public interface IEmailSender
{
    Task SendContactMessageAsync(
        string senderName,
        string senderEmail,
        string? subject,
        string message,
        CancellationToken cancellationToken
    );

    Task SendPasswordResetOtpAsync(
        string recipientEmail,
        string recipientName,
        string otp,
        CancellationToken cancellationToken
    );
}

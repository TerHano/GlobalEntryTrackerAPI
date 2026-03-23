using System.Text.Encodings.Web;
using Database.Entities;
using Microsoft.AspNetCore.Identity;
using Resend;

namespace Service;

public class ResendEmailSender(IResend resend) : IEmailSender<UserEntity>
{
    public async Task SendConfirmationLinkAsync(UserEntity user, string email,
        string confirmationLink)
    {
        var encodedLink = HtmlEncoder.Default.Encode(confirmationLink);
        var message = new EmailMessage
        {
            From = "EntryAlert <no-reply@terhano.com>"
        };
        message.To.Add(email);
        message.Subject = "Email Confirmation";
        message.HtmlBody =
            $"<p>Please confirm your email by clicking <a href=\"{encodedLink}\">here</a>.</p>";

        await resend.EmailSendAsync(message);
    }

    public async Task SendPasswordResetLinkAsync(UserEntity user, string email, string resetLink)
    {
        var message = new EmailMessage
        {
            From = "EntryAlert <no-reply@terhano.com>",
            To = { email },
            Subject = "Password Reset",
            HtmlBody =
                $"<p>You can reset your password by clicking <a href=\"{resetLink}\">here</a>.</p>"
        };
        await resend.EmailSendAsync(message);
    }

    public async Task SendPasswordResetCodeAsync(UserEntity user, string email, string resetCode)
    {
        var message = new EmailMessage
        {
            From = "EntryAlert <no-reply@terhano.com>",
            To = { email },
            Subject = "Password Reset Code",
            HtmlBody = $"Your password reset code is: {resetCode}"
        };
        await resend.EmailSendAsync(message);
    }
}
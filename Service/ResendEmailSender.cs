using Database.Entities;
using Microsoft.AspNetCore.Identity;
using Resend;

namespace Service;

public class ResendEmailSender(IResend resend) : IEmailSender<UserEntity>
{
    public async Task SendConfirmationLinkAsync(UserEntity user, string email,
        string confirmationLink)
    {
        var message = new EmailMessage
        {
            From = "EntryAlert <no-reply@terhano.com>"
        };
        message.To.Add(email);
        message.Subject = "Email Confirmation";
        message.HtmlBody = confirmationLink;

        await resend.EmailSendAsync(message);
    }

    public async Task SendPasswordResetLinkAsync(UserEntity user, string email, string resetLink)
    {
        var message = new EmailMessage
        {
            From = "EntryAlert <no-reply@terhano.com>",
            To = { email },
            Subject = "Password Reset",
            HtmlBody = resetLink
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
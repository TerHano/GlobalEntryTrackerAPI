using Database.Entities;
using Microsoft.AspNetCore.Identity;
using Resend;

namespace Service;

public class ResendEmailSender(IResend resend) : IEmailSender<UserEntity>
{
    private static async Task<string> LoadTemplateAsync(string templateName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", templateName);
        return await File.ReadAllTextAsync(path);
    }

    public async Task SendConfirmationLinkAsync(UserEntity user, string email,
        string confirmationLink)
    {
        var template = await LoadTemplateAsync("ConfirmEmailTemplate.html");
        var htmlBody = template
            .Replace("{UserName}", user.UserName ?? email)
            .Replace("{ConfirmationLink}", confirmationLink);

        var message = new EmailMessage
        {
            From = "EntryAlert <no-reply@terhano.com>",
            To = { email },
            Subject = "Confirm Your Email",
            HtmlBody = htmlBody
        };

        await resend.EmailSendAsync(message);
    }

    public async Task SendPasswordResetLinkAsync(UserEntity user, string email, string resetLink)
    {
        var template = await LoadTemplateAsync("ResetPasswordTemplate.html");
        var htmlBody = template
            .Replace("{UserName}", user.UserName ?? email)
            .Replace("{ResetPasswordLink}", resetLink);

        var message = new EmailMessage
        {
            From = "EntryAlert <no-reply@terhano.com>",
            To = { email },
            Subject = "Reset Your Password",
            HtmlBody = htmlBody
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
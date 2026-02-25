using System.Net.Mail;
using Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Service;

public class EmailSender
    : IEmailSender<UserEntity>
{
    private readonly IConfiguration _configuration;
    private readonly string _fromAddress;
    private readonly string _fromName;
    private readonly SmtpClient _smtpClient;

    public EmailSender(SmtpClient smtpClient, IConfiguration configuration)
    {
        _smtpClient = smtpClient;
        var fromAddress =
            configuration["Smtp:From_Address"] ?? null;
        var fromName =
            configuration["Smtp:From_Name"] ?? null;
        if (fromName == null || fromAddress == null)
            throw new ApplicationException(
                "From address or name is not set in environment variables");
        _fromAddress = fromAddress;
        _fromName = fromName;
    }

    public async Task SendConfirmationLinkAsync(UserEntity user, string email,
        string confirmationLink)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));
        if (string.IsNullOrEmpty(confirmationLink))
            throw new ArgumentNullException(nameof(confirmationLink));


        var message = new MailMessage
        {
            From = new MailAddress(_fromAddress, _fromName),
            To = { new MailAddress(email) },
            Subject = "Email Confirmation",
            Body =
                confirmationLink,
            IsBodyHtml = true
        };
        try
        {
            await _smtpClient.SendMailAsync(message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.StackTrace);
            throw new ApplicationException("Failed to send confirmation email", e);
        }
    }

    public Task SendPasswordResetLinkAsync(UserEntity user, string email, string resetLink)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));
        if (string.IsNullOrEmpty(resetLink))
            throw new ArgumentNullException(nameof(resetLink));

        var message = new MailMessage
        {
            From = new MailAddress(_fromAddress, _fromName),
            To = { new MailAddress(email) },
            Subject = "Password Reset",
            Body =
                $"Please reset your password by clicking this link: <a href=\"{resetLink}\">Reset Password</a>",
            IsBodyHtml = true
        };
        return _smtpClient.SendMailAsync(message);
    }

    public Task SendPasswordResetCodeAsync(UserEntity user, string email, string resetCode)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));
        if (string.IsNullOrEmpty(resetCode))
            throw new ArgumentNullException(nameof(resetCode));

        var message = new MailMessage
        {
            From = new MailAddress(_fromAddress, _fromName),
            To = { new MailAddress(email) },
            Subject = "Password Reset Code",
            Body =
                $"Your password reset code is: <strong>{resetCode}</strong>",
            IsBodyHtml = true
        };
        return _smtpClient.SendMailAsync(message);
    }
}
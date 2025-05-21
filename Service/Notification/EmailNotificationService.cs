using System.Net.Mail;
using System.Text;
using Database.Entities;
using Database.Entities.NotificationSettings;
using Database.Repositories;
using Microsoft.Extensions.Logging;
using Service.Dto;

namespace Service.Notification;

public class EmailNotificationService(
    SmtpClient smtpClient,
    UserNotificationRepository userNotificationRepository,
    ILogger<EmailNotificationService> logger) : INotificationService
{
    public async Task SendTestNotification<T>(T settingsToTest)
    {
        if (settingsToTest is string email)
        {
            var message = GenerateEmailMessage(email, "Test Email",
                "<h1>This is a test email</h1><p>If you see this, the email service is working.</p>");
            await smtpClient.SendMailAsync(message);
        }
    }

    public async Task SendNotification<T>(List<LocationAppointmentDto> appointments,
        AppointmentLocationEntity locationInformation, T emailNotificationSettings)
    {
        if (emailNotificationSettings is not EmailNotificationSettingsEntity
            emailNotificationSettingsEntity)
        {
            logger.LogError("User notification ID is null");
            return;
        }

        var userEmail = emailNotificationSettingsEntity.Email;
        var message =
            GenerateEmailMessageFromAppointment(appointments, locationInformation, userEmail);
        await smtpClient.SendMailAsync(message);
    }

    private MailMessage GenerateEmailMessageFromAppointment(
        List<LocationAppointmentDto> appointments,
        AppointmentLocationEntity locationInformation,
        string userEmail)
    {
        var body = GenerateEmailBody(appointments, locationInformation);
        var message = GenerateEmailMessage(userEmail, "Available Appointments",
            body);

        return message;
    }

    private string GenerateEmailBody(
        List<LocationAppointmentDto> appointments,
        AppointmentLocationEntity locationInformation)
    {
        var body = new StringBuilder();
        body.AppendLine($"<h1>Available Appointments for {locationInformation.Name}</h1>");
        body.AppendLine("<ul>");

        foreach (var appointment in appointments)
            body.AppendLine($"<li>{appointment.StartTimestamp} - {appointment.EndTimestamp}</li>");

        body.AppendLine("</ul>");

        return body.ToString();
    }

    private MailMessage GenerateEmailMessage(string to, string subject, string body)
    {
        var fromAddress =
            Environment.GetEnvironmentVariable("Smtp__FromAddress");
        var fromName =
            Environment.GetEnvironmentVariable("Smtp__FromName");
        if (fromAddress == null || fromName == null)
            throw new ApplicationException(
                "From address or name is not set in environment variables");

        var message = new MailMessage
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
            To = { new MailAddress(to) },
            From = new MailAddress(fromAddress, fromName)
        };

        return message;
    }
}
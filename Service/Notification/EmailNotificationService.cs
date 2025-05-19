using System.Net.Mail;
using System.Text;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.Logging;
using Service.Dto;

namespace Service.Notification;

public class EmailNotificationService(
    SmtpClient smtpClient,
    UserNotificationRepository userNotificationRepository,
    ILogger<EmailNotificationService> logger) : INotificationService
{
    public async Task SendNotification(List<LocationAppointmentDto> appointments,
        AppointmentLocationEntity locationInformation, int? userNotificationId)
    {
        if (userNotificationId == null)
        {
            logger.LogError("User notification ID is null");
            return;
        }

        var userNotification =
            await userNotificationRepository.GetUserWithNotificationSettings(userNotificationId
                .Value);
        var user = userNotification.User;
        if (user == null)
            throw new ApplicationException("No user found");
        if (user.Email == null)
            throw new ApplicationException("No email found for user");
        var userEmail = user.Email;
        var message =
            GenerateEmailMessageFromAppointment(appointments, locationInformation, userEmail);
        await smtpClient.SendMailAsync(message);
    }

    public async Task SendTestNotification<T>(T settingsToTest)
    {
        if (settingsToTest is string email)
        {
            var message = GenerateTestEmailMessage(email);
            await smtpClient.SendMailAsync(message);
        }
    }

    private MailMessage GenerateEmailMessageFromAppointment(
        List<LocationAppointmentDto> appointments,
        AppointmentLocationEntity locationInformation,
        string userEmail)
    {
        var fromAddress =
            Environment.GetEnvironmentVariable("Smtp__FromAddress");
        if (fromAddress == null)
            throw new ApplicationException("From address is not set in environment variables");

        var message = new MailMessage
        {
            Subject = "Available Appointments",
            Body = GenerateEmailBody(appointments, locationInformation),
            IsBodyHtml = true,
            To = { new MailAddress(userEmail) },
            From = new MailAddress(fromAddress)
        };

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

    private MailMessage GenerateTestEmailMessage(
        string userEmail)
    {
        var fromAddress =
            Environment.GetEnvironmentVariable("Smtp__FromAddress");
        if (fromAddress == null)
            throw new ApplicationException("From address is not set in environment variables");

        var message = new MailMessage
        {
            Subject = "Test Email",
            Body = "This is a test email.",
            IsBodyHtml = true,
            To = { new MailAddress(userEmail) },
            From = new MailAddress(fromAddress)
        };

        return message;
    }
}
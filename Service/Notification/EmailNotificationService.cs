using System.Net;
using System.Net.Mail;
using System.Text;
using Database.Entities;
using Database.Entities.NotificationSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service.Dto;

namespace Service.Notification;

public class EmailNotificationService(
    SmtpClient smtpClient,
    IConfiguration configuration,
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
        AppointmentLocationEntity locationInformation, T emailNotificationSettings,
        NotificationDeliveryContext deliveryContext)
    {
        if (emailNotificationSettings is not EmailNotificationSettingsEntity
            emailNotificationSettingsEntity)
        {
            logger.LogError("User notification ID is null");
            return;
        }

        var userEmail = emailNotificationSettingsEntity.Email;
        var message =
            GenerateEmailMessageFromAppointment(appointments, locationInformation, userEmail,
                deliveryContext);
        await smtpClient.SendMailAsync(message);
    }

    private MailMessage GenerateEmailMessageFromAppointment(
        List<LocationAppointmentDto> appointments,
        AppointmentLocationEntity locationInformation,
        string userEmail,
        NotificationDeliveryContext deliveryContext)
    {
        var body = GenerateEmailBody(appointments, locationInformation, deliveryContext);
        var message = GenerateEmailMessage(userEmail, "Available Appointments",
            body);

        return message;
    }

    private string GenerateEmailBody(
        List<LocationAppointmentDto> appointments,
        AppointmentLocationEntity locationInformation,
        NotificationDeliveryContext deliveryContext)
    {
        var body = new StringBuilder();
        body.AppendLine(
            $"<h1>Available Appointments for {WebUtility.HtmlEncode(locationInformation.Name)}</h1>");
        body.AppendLine("<ul>");

        foreach (var appointmentsForDay in appointments.GroupBy(x => x.StartTimestamp.Date))
        {
            var times = string.Join(", ",
                appointmentsForDay.Select(x => x.StartTimestamp.ToString("h:mm tt")));
            body.AppendLine($"<li><strong>{appointmentsForDay.Key:MMM dd, yyyy}</strong>: {times}</li>");
        }

        body.AppendLine("</ul>");
        body.AppendLine(
            "<p><a href=\"https://ttp.cbp.dhs.gov/dashboard\">Schedule your appointment</a></p>");
        body.AppendLine(GenerateDelayNotice(deliveryContext));

        return body.ToString();
    }

    private static string GenerateDelayNotice(NotificationDeliveryContext deliveryContext)
    {
        if (!deliveryContext.IsDelayed) return string.Empty;

        var notice = new StringBuilder();
        notice.AppendLine("<hr />");
        notice.AppendLine(
            $"<p><strong>This alert was held for {deliveryContext.DelayInMinutes} minutes.</strong> " +
            "Your plan delays appointment alerts, so subscribers have already seen these. " +
            "Subscribers are alerted the moment an appointment appears.</p>");
        if (deliveryContext.UpgradeUrl != null)
            notice.AppendLine(
                $"<p><a href=\"{WebUtility.HtmlEncode(deliveryContext.UpgradeUrl)}\">Upgrade for instant alerts</a></p>");
        return notice.ToString();
    }

    private MailMessage GenerateEmailMessage(string to, string subject, string body)
    {
        var fromAddress =
            configuration["Smtp:From_Address"] ?? null;
        var fromName =
            configuration["Smtp:From_Name"] ?? null;
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
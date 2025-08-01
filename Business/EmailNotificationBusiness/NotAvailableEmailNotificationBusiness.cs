using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using Business.Exceptions;

namespace Business;

public class NotAvailableEmailNotificationBusiness : IEmailNotificationSettingsBusiness
{
    public Task<EmailNotificationSettingsDto?> GetEmailNotificationSettingsForUser(string userId)
    {
        throw new NotAvailableException("Email notification settings are not available.");
    }

    public Task<int> CreateEmailNotificationSettingsForUser(
        CreateEmailNotificationSettingsRequest settings,
        string userId)
    {
        throw new NotAvailableException("Email notification settings are not available.");
    }

    public Task<int> UpdateEmailNotificationSettingsForUser(
        UpdateEmailNotificationSettingsRequest settings,
        string userId)
    {
        throw new NotAvailableException("Email notification settings are not available.");
    }

    public Task SendEmailTestMessage(string userId)
    {
        throw new NotAvailableException("Email notification settings are not available.");
    }
}
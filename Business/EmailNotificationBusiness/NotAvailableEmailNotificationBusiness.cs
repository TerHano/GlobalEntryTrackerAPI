using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using Business.Exceptions;

namespace Business;

public class NotAvailableEmailNotificationBusiness : IEmailNotificationSettingsBusiness
{
    public Task<EmailNotificationSettingsDto?> GetEmailNotificationSettingsForUser(int userId)
    {
        throw new NotAvailableException("Email notification settings are not available.");
    }

    public Task<int> CreateEmailNotificationSettingsForUser(
        CreateEmailNotificationSettingsRequest settings,
        int userId)
    {
        throw new NotAvailableException("Email notification settings are not available.");
    }

    public Task<int> UpdateEmailNotificationSettingsForUser(
        UpdateEmailNotificationSettingsRequest settings,
        int userId)
    {
        throw new NotAvailableException("Email notification settings are not available.");
    }

    public Task SendEmailTestMessage(int userId)
    {
        throw new NotAvailableException("Email notification settings are not available.");
    }
}
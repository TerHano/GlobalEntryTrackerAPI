using Business.Dto.NotificationSettings;
using Business.Dto.Requests;

namespace Business;

public interface IEmailNotificationSettingsBusiness
{
    /// <summary>
    ///     Gets the email notification settings for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>Email notification settings DTO or null.</returns>
    public Task<EmailNotificationSettingsDto?> GetEmailNotificationSettingsForUser(
        int userId);


    /// <summary>
    ///     Creates email notification settings for a user.
    /// </summary>
    /// <param name="settings">Settings request.</param>
    /// <param name="userId">User ID.</param>
    /// <returns>ID of the created settings.</returns>
    public Task<int> CreateEmailNotificationSettingsForUser(
        CreateEmailNotificationSettingsRequest settings, int userId);

    /// <summary>
    ///     Updates email notification settings for a user.
    /// </summary>
    /// <param name="settings">Settings request.</param>
    /// <param name="userId">User ID.</param>
    /// <returns>ID of the updated settings.</returns>
    public Task<int> UpdateEmailNotificationSettingsForUser(
        UpdateEmailNotificationSettingsRequest settings, int userId);

    /// <summary>
    ///     Sends a test email message to the user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    public Task SendEmailTestMessage(int userId);
}
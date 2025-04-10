using System.Net;
using System.Text;
using System.Text.Json;
using Database.Entities.NotificationSettings;
using Database.Repositories;
using Microsoft.Extensions.Logging;
using Service.Dto;

namespace Service.Notification;

public class DiscordNotificationService(
    ILogger<DiscordNotificationService> logger,
    DiscordNotificationSettingsRepository discordNotificationSettingsRepository,
    HttpClient httpClient)
    : INotificationService
{
    public async Task SendNotification(
        LocationAppointmentWithDetailsDto appointment, int userId)
    {
        var settings =
            await discordNotificationSettingsRepository.GetNotificationSettingsForUser(userId);
        if (settings is { Enabled: true })
        {
            var message = GenerateDiscordMessageFromAppointment(appointment);
            await SendMessageThroughWebhook(settings, message);
        }
    }

    public async Task SendTestNotification<T>(T settingsToTest)
    {
        if (settingsToTest != null && settingsToTest is DiscordNotificationSettingsEntity settings)
        {
            var testMessage = GenerateTestMessage();
            await SendMessageThroughWebhook(settings, testMessage);
        }
        else
        {
            logger.LogError("Settings passed is not of type DiscordNotificationSettingsEntity");
        }
    }


    private async Task SendMessageThroughWebhook(DiscordNotificationSettingsEntity settings,
        DiscordWebhookMessageDto message)
    {
        var messageJson = JsonSerializer.Serialize(message);
        var request = new StringContent(messageJson, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(settings.WebhookUrl, request);
        if (response.StatusCode != HttpStatusCode.NoContent) throw new Exception();
    }

    private DiscordWebhookMessageDto GenerateTestMessage()
    {
        var testMessage = new DiscordWebhookMessageDto
        {
            Username = "Global Entry Alert Bot",
            Embeds =
            [
                new DiscordWebhookMessageDto.Embed
                {
                    Title = "This Is A Test Message",
                    Description =
                        "If You're Seeing This, It Worked!",
                    Color = 16761600
                }
            ]
        };
        return testMessage;
    }

    private DiscordWebhookMessageDto GenerateDiscordMessageFromAppointment(
        LocationAppointmentWithDetailsDto appointment)
    {
        var discordMessage = new DiscordWebhookMessageDto
        {
            Username = "Global Entry Alert Bot",
            Embeds =
            [
                new DiscordWebhookMessageDto.Embed
                {
                    Title = "New Appointment Available",
                    Description =
                        "Click [here](https://ttp.cbp.dhs.gov/dashboard) to schedule your appointment",
                    Color = 16761600,
                    Fields =
                    [
                        new DiscordWebhookMessageDto.Field
                        {
                            Name = "Location",
                            Value = appointment.Location.Name
                        },

                        new DiscordWebhookMessageDto.Field
                        {
                            Name = "Appointment",
                            Value = appointment.StartTimestamp.ToShortDateString()
                        }
                    ]
                }
            ]
        };
        return discordMessage;
    }
}
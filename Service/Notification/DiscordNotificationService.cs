using System.Net;
using System.Text;
using System.Text.Json;
using Business.Dto.Requests;
using Database.Entities;
using Database.Entities.NotificationSettings;
using Microsoft.Extensions.Logging;
using Service.Dto;

namespace Service.Notification;

public class DiscordNotificationService(
    ILogger<DiscordNotificationService> logger,
    HttpClient httpClient)
    : INotificationService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task SendNotification<T>(
        List<LocationAppointmentDto> appointments, AppointmentLocationEntity locationInformation,
        T userNotificationSettings, NotificationDeliveryContext deliveryContext)
    {
        if (userNotificationSettings is null ||
            userNotificationSettings is not DiscordNotificationSettingsEntity
                discordNotificationSettings)
        {
            logger.LogError("User notification ID is null");
            return;
        }

        if (discordNotificationSettings is { Enabled: true })
        {
            var message = GenerateDiscordMessageFromAppointment(appointments, locationInformation,
                deliveryContext);
            await SendMessageThroughWebhook(discordNotificationSettings, message);
        }
    }

    public async Task SendTestNotification<T>(T settingsToTest)
    {
        if (settingsToTest != null && settingsToTest is TestDiscordSettingsRequest settings)
        {
            var testMessage = GenerateTestMessage();
            await SendMessageThroughWebhook(settings, testMessage);
        }
        else
        {
            logger.LogError("Settings passed is not of type TestDiscordSettingsRequest");
        }
    }

    private List<DiscordWebhookMessageDto.Field> GetAvailableAppointmentFields(
        List<LocationAppointmentDto> appointments,
        AppointmentLocationEntity locationInformation)
    {
        var groupedAppointments = appointments
            .GroupBy(appointment => appointment.StartTimestamp.Date)
            .ToDictionary(group => group.Key, group => group.ToList());

        var fields = new List<DiscordWebhookMessageDto.Field>
        {
            new()
            {
                Name = "Location",
                Value = locationInformation.Name
            }
        };
        foreach (var (date, appointmentList) in groupedAppointments)
        {
            var dateField = new DiscordWebhookMessageDto.Field
            {
                Name = date.ToString("MMM dd, yyyy"),
                Value = string.Join(", ",
                    appointmentList.Select(a => a.StartTimestamp.ToString("hh:mm tt")))
            };
            fields.Add(dateField);
        }

        return fields;
    }


    private async Task SendMessageThroughWebhook(DiscordNotificationSettingsEntity settings,
        DiscordWebhookMessageDto message)
    {
        var messageJson = JsonSerializer.Serialize(message, _jsonSerializerOptions);
        var request = new StringContent(messageJson, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(settings.WebhookUrl, request);
        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Discord webhook failed with status {StatusCode}: {ErrorContent}", 
                response.StatusCode, errorContent);
            throw new Exception($"Discord webhook request failed with status {response.StatusCode}");
        }
    }

    private async Task SendMessageThroughWebhook(TestDiscordSettingsRequest settings,
        DiscordWebhookMessageDto message)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        var messageJson = JsonSerializer.Serialize(message, options);
        logger.LogInformation(messageJson);
        var request = new StringContent(messageJson, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(settings.WebhookUrl, request);
        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Discord test webhook failed with status {StatusCode}: {ErrorContent}", 
                response.StatusCode, errorContent);
            throw new Exception($"Discord webhook request failed with status {response.StatusCode}");
        }
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
        List<LocationAppointmentDto> appointments, AppointmentLocationEntity locationInformation,
        NotificationDeliveryContext deliveryContext)
    {
        var fields = GetAvailableAppointmentFields(appointments, locationInformation);

        if (deliveryContext.IsDelayed)
            fields.Add(new DiscordWebhookMessageDto.Field
            {
                Name = $"⏳ Sent {deliveryContext.DelayInMinutes} minutes late",
                Value = BuildUpgradeMessage(deliveryContext)
            });

        var discordMessage = new DiscordWebhookMessageDto
        {
            Username = "Global Entry Alert Bot",
            Embeds =
            [
                new DiscordWebhookMessageDto.Embed
                {
                    Title = "New Appointment(s) Available",
                    Description =
                        "Click [here](https://ttp.cbp.dhs.gov/dashboard) to schedule your appointment",
                    Color = 16761600,
                    Fields = fields
                }
            ]
        };
        return discordMessage;
    }

    private static string BuildUpgradeMessage(NotificationDeliveryContext deliveryContext)
    {
        var message =
            $"Your plan holds alerts for {deliveryContext.DelayInMinutes} minutes, so these " +
            "appointments have already been available to subscribers. Subscribers are alerted the " +
            "moment an appointment appears.";
        return deliveryContext.UpgradeUrl == null
            ? message
            : $"{message} [Upgrade for instant alerts]({deliveryContext.UpgradeUrl})";
    }
}
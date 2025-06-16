using System.Text.Json.Serialization;

namespace Business.Dto;

public class SupabaseResendResponse
{
    [JsonPropertyName("message_id")] public string MessageId { get; set; }
}
using System.Text.Json.Serialization;

namespace Business.Dto;

public class SupabaseRefreshTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }

    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; }
}
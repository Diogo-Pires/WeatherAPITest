using System.Text.Json.Serialization;

namespace Application.DTOs;

public class Weather
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("main")]
    public required string Main { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("icon")]
    public required string Icon { get; set; }
}
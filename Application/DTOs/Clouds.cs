using System.Text.Json.Serialization;

namespace Application.DTOs;

public class Clouds
{
    [JsonPropertyName("all")]
    public int All { get; set; }
}
using System.Text.Json.Serialization;

namespace Application.DTOs;

public class Root
{
    [JsonPropertyName("coord")]
    public required Coord Coord { get; set; }

    [JsonPropertyName("weather")]
    public required List<Weather> Weather { get; set; }

    [JsonPropertyName("base")]
    public required string Base { get; set; }

    [JsonPropertyName("main")]
    public required Main Main { get; set; }

    [JsonPropertyName("visibility")]
    public int Visibility { get; set; }

    [JsonPropertyName("wind")]
    public required Wind Wind { get; set; }

    [JsonPropertyName("clouds")]
    public required Clouds Clouds { get; set; }

    [JsonPropertyName("dt")]
    public int Dt { get; set; }

    [JsonPropertyName("sys")]
    public required Sys Sys { get; set; }

    [JsonPropertyName("timezone")]
    public int Timezone { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("cod")]
    public int Cod { get; set; }
}
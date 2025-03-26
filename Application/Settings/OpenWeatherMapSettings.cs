namespace Application.Settings;

public record OpenWeatherMapSettings
{
    public required string Url { get; set; }
    public required string APIKEY { get; set; }
}

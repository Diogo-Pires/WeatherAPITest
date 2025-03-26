using Application.DTOs;
using Application.Interfaces;
using Application.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Application.Services;

public class OpenWeatherMapService(IHttpClientFactory factory, 
                                   IOptions<OpenWeatherMapSettings> options) : IOpenWeatherMapService
{
    private readonly IHttpClientFactory _factory = factory;
    private readonly OpenWeatherMapSettings _options = options.Value;

    public async Task<Root?> GetWeatherContentAsync()
    {
        using var client = _factory.CreateClient("openweather");
        var logs = await client
            .GetFromJsonAsync<Root>($"weather?q=London&appid={_options.APIKEY}");

        return logs;
    }
}

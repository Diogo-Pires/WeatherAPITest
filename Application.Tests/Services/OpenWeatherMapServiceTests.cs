using Application.DTOs;
using Application.Services;
using Application.Settings;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;

public class OpenWeatherMapServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IOptions<OpenWeatherMapSettings>> _optionsMock;
    private readonly OpenWeatherMapService _service;

    public OpenWeatherMapServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _optionsMock = new Mock<IOptions<OpenWeatherMapSettings>>();
        _optionsMock.Setup(opt => opt.Value).Returns(new OpenWeatherMapSettings { Url = "http://dummy.com", APIKEY = "test-api-key" });
        _service = new OpenWeatherMapService(_httpClientFactoryMock.Object, _optionsMock.Object);
    }

    [Fact]
    public async Task GetWeatherContentAsync_ShouldReturnWeatherData_WhenResponseIsSuccessful()
    {
        // Arrange
        var expectedWeather = new Root
        {
            Coord = new Coord { Lat = 51.5074, Lon = -0.1278 },
            Weather =
            [
                new Weather { Id = 800, Main = "Clear", Description = "clear sky", Icon = "01d" }
            ],
            Base = "stations",
            Main = new Main
            {
                Temp = 15.5,
                FeelsLike = 14.8,
                TempMin = 12.0,
                TempMax = 17.2,
                Pressure = 1012,
                Humidity = 60
            },
            Visibility = 10000,
            Wind = new Wind { Speed = 3.5, Deg = 220 },
            Clouds = new Clouds { All = 0 },
            Dt = 1711363200,
            Sys = new Sys { Country = "GB", Sunrise = 1711327200, Sunset = 1711370400 },
            Timezone = 0,
            Id = 2643743,
            Name = "London",
            Cod = 200
        };
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedWeather)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        httpClient.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");

        _httpClientFactoryMock
            .Setup(factory => factory.CreateClient("openweather"))
            .Returns(httpClient);

        // Act
        var result = await _service.GetWeatherContentAsync();

        // Assert
        Assert.NotNull(result); 
        Assert.Equal(51.5074, result.Coord.Lat);
        Assert.Equal(-0.1278, result.Coord.Lon);
        Assert.Single(result.Weather);
        Assert.Equal("Clear", result.Weather[0].Main);
        Assert.Equal("clear sky", result.Weather[0].Description);
        Assert.Equal("01d", result.Weather[0].Icon);
        Assert.Equal("stations", result.Base);
        Assert.Equal(15.5, result.Main.Temp);
        Assert.Equal(1012, result.Main.Pressure);
        Assert.Equal(10000, result.Visibility);
        Assert.Equal(3.5, result.Wind.Speed);
        Assert.Equal("GB", result.Sys.Country);
        Assert.Equal("London", result.Name);
        Assert.Equal(200, result.Cod);
    }

    [Fact]
    public async Task GetWeatherContentAsync_ShouldReturnNull_WhenResponseIsNotSuccessful()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        httpClient.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");

        _httpClientFactoryMock
            .Setup(factory => factory.CreateClient("openweather"))
            .Returns(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetWeatherContentAsync());
        Assert.Equal("Response status code does not indicate success: 400 (Bad Request).", exception.Message);
    }
}

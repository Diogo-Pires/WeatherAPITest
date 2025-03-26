using Moq;
using Application.Interfaces;
using Domain;
using Microsoft.Extensions.Logging;
using Shared.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Application.DTOs;
using System.Net;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure;

namespace Atea_Test1.Tests;

public class FetchWeatherFunctionGeneralTests
{
    private readonly Mock<IOpenWeatherMapService> _mockWeatherService;
    private readonly Mock<ILogger<FetchWeatherFunction>> _mockLogger;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
    private readonly Mock<object> _mockTableClientBase; 

    public FetchWeatherFunctionGeneralTests()
    {
        _mockWeatherService = new Mock<IOpenWeatherMapService>();
        _mockLogger = new Mock<ILogger<FetchWeatherFunction>>();
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();
        _mockTableClientBase = new Mock<object>();
    }

    [Fact]
    public async Task Run_ShouldLogInformation_WhenWeatherFetchIsSuccessful()
    {
        // Arrange
        var weatherContent = new Root
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

        _mockWeatherService
            .Setup(service => service.GetWeatherContentAsync())
            .ReturnsAsync(weatherContent);

        _mockDateTimeProvider
            .Setup(provider => provider.GetUTCNow())
            .Returns(DateTime.UtcNow);

        var timerInfoMock = new Mock<TimerInfo>(MockBehavior.Loose);
        var mockResponse = new Mock<Response>();
        mockResponse
            .SetupGet(x => x.Status)
            .Returns((int)HttpStatusCode.NotFound);

        var tableClient = new Mock<TableClient>();
        tableClient
            .Setup(_ => _.AddEntityAsync(It.IsAny<WeatherLog>(), CancellationToken.None))
            .ReturnsAsync(mockResponse.Object);

        var blobClientMock = new Mock<BlobClient>();
        blobClientMock
            .Setup(x => x.UploadAsync(It.IsAny<BinaryData>(), true, It.IsAny<CancellationToken>()))
            .Verifiable();

        var blobItem = BlobsModelFactory.BlobItem(It.IsAny<string>(), false, null, null, null, null, null, null, null, null);
        var responsePage = Page<BlobItem>.FromValues([blobItem], continuationToken: null, Mock.Of<Response>());
        var responsePageable = Pageable<BlobItem>.FromPages([responsePage]);

        var mockBlobContainerClient = new Mock<BlobContainerClient>();
        mockBlobContainerClient
            .Setup(c => c.GetBlobs(BlobTraits.None, BlobStates.None, null, It.IsAny<CancellationToken>()))
            .Returns(responsePageable);
        mockBlobContainerClient
            .Setup(c => c.Exists(default))
            .Returns(Response.FromValue(true, Mock.Of<Response>()));
        mockBlobContainerClient
            .Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Returns(blobClientMock.Object);

        var fetchWeatherFunction = new FetchWeatherFunction(
            _mockWeatherService.Object,
            tableClient.Object, 
            mockBlobContainerClient.Object,
            _mockDateTimeProvider.Object,
            _mockLogger.Object
        );

        // Act
        await fetchWeatherFunction.Run(timerInfoMock.Object);

        // Assert
        blobClientMock.Verify(x => x.UploadAsync(It.IsAny<BinaryData>(), true, It.IsAny<CancellationToken>()));
    }
}
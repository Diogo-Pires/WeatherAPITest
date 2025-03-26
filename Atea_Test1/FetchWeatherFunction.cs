using Application.Interfaces;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Shared.Interfaces;
using System.Net;

namespace Atea_Test1;

public class FetchWeatherFunction
{
    private readonly TableClient _tableClient;
    private readonly BlobContainerClient _blobContainerClient; 
    private readonly IOpenWeatherMapService _openWeatherMapService;
    private readonly ILogger<FetchWeatherFunction> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly Lazy<Task> _initializeResources;

    public FetchWeatherFunction(
        IOpenWeatherMapService openWeatherMapService,
        TableClient tableClient,
        BlobContainerClient blobContainerClient,
        IDateTimeProvider dateTimeProvider,
        ILogger<FetchWeatherFunction> logger)
    {
        _openWeatherMapService = openWeatherMapService;
        _tableClient = tableClient;
        _blobContainerClient = blobContainerClient;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger; 
        _initializeResources = new Lazy<Task>(EnsureResourcesExistsAsync);
    }

    [Function("FetchWeather")]
    public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("Function 'FetchWeather' triggered at {Time}", _dateTimeProvider.GetUTCNow());
        await _initializeResources.Value;

        var log = new WeatherLog{ Timestamp = _dateTimeProvider.GetUTCNow() };

        try
        {
            _logger.LogInformation("Fetching weather data from OpenWeatherMap API...");
            var content = await _openWeatherMapService.GetWeatherContentAsync() ?? throw new Exception("No data found");

            log.Success = true;
            log.BlobName = $"{log.RowKey}.json";

            var blobClient = _blobContainerClient.GetBlobClient(log.BlobName);
            await blobClient.UploadAsync(new BinaryData(content), overwrite: true, CancellationToken.None);
            _logger.LogInformation("Weather data saved to Blob Storage with name: {BlobName}", log.BlobName);
        }
        catch (Exception ex)
        {
            log.Success = false;
            log.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Failed to fetch and store weather data.");
        }
        finally
        {
            await _tableClient.AddEntityAsync(log); 
            _logger.LogInformation("Log entry stored in Table Storage: Success={Success}, Timestamp={Timestamp}",log.Success, log.Timestamp);
        }
    }

    /// <summary>
    /// Gets Weather Logs
    /// </summary>
    /// <returns><see cref="List<WeatherLog>"/></returns>
    /// <remarks>
    /// Usage Example:
    /// GET api/logs/
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    [OpenApiOperation(operationId: nameof(GetWeatherLogs), tags: "logs")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<WeatherLog>), Description = "Gets Weather Logs")]
    [Function("GetWeatherLogs")]
    public async Task<IActionResult> GetWeatherLogs(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "logs")] HttpRequest req)
    {
        _logger.LogInformation("Received request to fetch logs.");

        if (req == null)
        {
            return new BadRequestObjectResult("Invalid request.");
        }

        string? fromStr = req.Query["from"];
        string? toStr = req.Query["to"];

        if (!DateTime.TryParse(fromStr, out var from) || !DateTime.TryParse(toStr, out var to))
        {
            _logger.LogWarning("Invalid date range received: from={From}, to={To}", fromStr, toStr);
            return new BadRequestObjectResult("Invalid arguments format.");
        }

        var logs = new List<WeatherLog>();
        await foreach (var log in _tableClient.QueryAsync<WeatherLog>(
            filter: $"Timestamp ge datetime'{from:O}' and Timestamp le datetime'{to:O}'"))
        {
            logs.Add(log);
        }

        _logger.LogInformation("Returning {Count} logs for the period {From} to {To}.", logs.Count, from, to);
        return new OkObjectResult(logs);
    }

    /// <summary>
    /// Gets Weather Payload by ID
    /// </summary>
    /// <param name="nameof(id)"></param>
    /// <returns><see cref="FileStreamResult"/></returns>
    /// <remarks>
    /// Usage Example:
    /// GET api/logs/{id}/payload
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="200">Ok</response>
    /// <response code="404">Not Found</response>
    /// <response code="400">Bad Request</response>
    [OpenApiOperation(operationId: nameof(GetWeatherPayload), tags: "logs/{id}/payload")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<WeatherLog>), Description = "Gets Weather Payload by ID")]
    [Function("GetWeatherPayload")]
    public async Task<IActionResult> GetWeatherPayload(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "logs/{id}/payload")] HttpRequest req, string id)
    {
        _logger.LogInformation("Received request to fetch payload for log ID: {Id}", id);

        if (req == null)
        {
            return new BadRequestObjectResult("Invalid request.");
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return new BadRequestObjectResult($"Invalid id: {id}");
        }

        var blobClient = _blobContainerClient.GetBlobClient($"{id}.json");

        if (!await blobClient.ExistsAsync())
        {
            _logger.LogWarning("Blob not found for log ID: {Id}", id);
            return new NotFoundResult();
        }

        var content = await blobClient.DownloadContentAsync();
        _logger.LogInformation("Successfully retrieved weather payload for log ID: {Id}", id);
        return new FileStreamResult(content.Value.Content.ToStream(), "application/json");
    }

    private async Task EnsureResourcesExistsAsync()
    {
        _logger.LogInformation("Checking and creating necessary resources...");

        await _tableClient.CreateIfNotExistsAsync();
        await _blobContainerClient.CreateIfNotExistsAsync();

        _logger.LogInformation("Resources initialized successfully.");
    }
}
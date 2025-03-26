using Application.Services;
using Application.Settings;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using Shared;
using Shared.Interfaces;
using Application.Interfaces;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.Configuration.AddUserSecrets<Program>();
builder.Services.Configure<OpenWeatherMapSettings>(builder.Configuration.GetSection("OpenWeatherMapSettings"));

var fallbackPolicy = Policy<HttpResponseMessage>
    .Handle<HttpRequestException>()
    .OrResult(response => response.StatusCode == HttpStatusCode.ServiceUnavailable)
    .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent(string.Empty)
    });

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

builder.Services.AddHttpClient<IOpenWeatherMapService, OpenWeatherMapService>("openweather", (serviceProvider, client) =>
{
    var settings = serviceProvider
        .GetRequiredService<IOptions<OpenWeatherMapSettings>>().Value;

    client.BaseAddress = new Uri(settings.Url);
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(fallbackPolicy.WrapAsync(retryPolicy));

string storageConnectionString = builder.Configuration["AzureWebJobsStorage"]
    ?? throw new InvalidOperationException("AzureWebJobsStorage is not configured.");

builder.Services.AddSingleton(_ => new TableClient(storageConnectionString, builder.Configuration["TableClientWeather"]));
builder.Services.AddSingleton(_ => new BlobContainerClient(storageConnectionString, builder.Configuration["BlobContainer"]));
builder.Services.AddSingleton<IOpenWeatherMapService, OpenWeatherMapService>();
builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();
builder.Build().Run();

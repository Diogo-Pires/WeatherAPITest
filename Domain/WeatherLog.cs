using Azure;
using Azure.Data.Tables;

namespace Domain;

public class WeatherLog : ITableEntity
{
    public string PartitionKey { get; set; } = "WeatherLogs";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string BlobName { get; set; }
    public string ErrorMessage { get; set; }
    public ETag ETag { get; set; }
    DateTimeOffset? ITableEntity.Timestamp { get; set; }
}
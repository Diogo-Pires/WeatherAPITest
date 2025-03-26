using Shared.Interfaces;

namespace Shared;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetUTCNow() => DateTime.Now;
}
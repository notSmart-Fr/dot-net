using TaskApi.Shared.Interfaces;

namespace TaskApi.Shared.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;
}
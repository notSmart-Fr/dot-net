namespace TaskApi.Shared.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTimeOffset UtcNowOffset { get; }
}
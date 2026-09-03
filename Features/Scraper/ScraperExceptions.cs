using TaskApi.Shared.ExceptionHandling;

namespace TaskApi.Features.Scraper;

public class ScraperRateLimitException() : DomainException(
    message: "Rate limit hit (HTTP 429). Backoff required.",
    statusCode: StatusCodes.Status429TooManyRequests,
    title: "Scraper Rate Limit Exceeded",
    errorCode: "SCRAPER_RATE_LIMIT");

public class HtmlParsingException(string message) : DomainException(
    message: message,
    statusCode: StatusCodes.Status422UnprocessableEntity,
    title: "HTML Parsing Error",
    errorCode: "SCRAPER_PARSE_ERROR");
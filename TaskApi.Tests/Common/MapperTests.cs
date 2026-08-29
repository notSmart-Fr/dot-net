using FluentAssertions;
using Microsoft.AspNetCore.Http;
using TaskApi.Common.Exceptions.Mappers;

namespace TaskApi.Tests.Common;

public class MapperTests
{
    private readonly DefaultHttpContext _httpContext = new();

    [Fact]
    public void BadRequestExceptionMapper_Handles_BadHttpRequestException_Correctly()
    {
        var mapper = new BadRequestExceptionMapper();
        var exception = new BadHttpRequestException("Invalid JSON syntax.");
        var traceId = "trace-123";

        var canHandle = mapper.CanHandle(exception);
        var (statusCode, details) = mapper.Map(_httpContext, exception, traceId);

        canHandle.Should().BeTrue();
        statusCode.Should().Be(StatusCodes.Status400BadRequest);
        details.Title.Should().Be("Bad Request");
        details.Extensions.Should().ContainKey("traceId").WhoseValue.Should().Be(traceId);
    }

    [Fact]
    public void UnauthorizedExceptionMapper_Handles_UnauthorizedAccessException_Correctly()
    {
        var mapper = new UnauthorizedExceptionMapper();
        var exception = new UnauthorizedAccessException("Unauthorized attempt.");
        var traceId = "trace-456";

        var canHandle = mapper.CanHandle(exception);
        var (statusCode, details) = mapper.Map(_httpContext, exception, traceId);

        canHandle.Should().BeTrue();
        statusCode.Should().Be(StatusCodes.Status401Unauthorized);
        details.Title.Should().Be("Unauthorized");
        details.Extensions.Should().ContainKey("traceId").WhoseValue.Should().Be(traceId);
    }
}
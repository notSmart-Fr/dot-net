using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using TaskApi.Core.Interfaces;
using TaskApi.Infrastructure.ExceptionHandling;
using Xunit;

namespace TaskApi.Tests.Infrastructure;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_DelegatesToMatchingMapper_AndWritesJsonResponse()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var mockException = new InvalidOperationException("Test exception");
        var expectedDetails = new ProblemDetails { Status = 400, Title = "Handled Exception" };

        var mockMapper = new TestMapper(canHandle: true, statusCode: 400, details: expectedDetails);
        var handler = new GlobalExceptionHandler(new[] { mockMapper }, NullLogger<GlobalExceptionHandler>.Instance);

        var result = await handler.TryHandleAsync(context, mockException, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(400);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        
        json.Should().Contain("Handled Exception");
    }

    [Fact]
    public async Task TryHandleAsync_FallsBackTo500_WhenNoMapperCanHandle()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var unhandledException = new InvalidCastException("Random unhandled crash");
        
        var mockMapper = new TestMapper(canHandle: false, statusCode: 400, details: new ProblemDetails());
        var handler = new GlobalExceptionHandler(new[] { mockMapper }, NullLogger<GlobalExceptionHandler>.Instance);

        var result = await handler.TryHandleAsync(context, unhandledException, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(500);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();

        json.Should().Contain("Internal Server Error");
    }

    private class TestMapper(bool canHandle, int statusCode, ProblemDetails details) : IExceptionMapper
    {
        public bool CanHandle(Exception exception) => canHandle;
        public (int StatusCode, ProblemDetails Details) Map(HttpContext context, Exception exception, string traceId) => (statusCode, details);
    }
}
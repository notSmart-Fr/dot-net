using FluentValidation;

namespace TaskApi.Common.Filters;

public class ValidationFilter<T>(IValidator<T>? validator = null) : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (validator is null) return await next(context);

        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        if (argument is null) return await next(context);

        var validationResult = await validator.ValidateAsync(argument, context.HttpContext.RequestAborted);
        if (validationResult.IsValid) return await next(context);

        var errors = validationResult.ToDictionary();

        var problemDetails = new HttpValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
            Title = "Validation Failed",
            Detail = "One or more validation errors occurred.",
            Instance = context.HttpContext.Request.Path
        };

        return Results.Problem(problemDetails);
    }
}
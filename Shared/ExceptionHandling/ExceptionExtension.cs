using TaskApi.Shared.Interfaces;

namespace TaskApi.Shared.ExceptionHandling;

public static class ExceptionExtensions
{
    public static IServiceCollection AddExceptionHandlingInfrastructure(this IServiceCollection services)
    {
        // Auto-discover all IExceptionMapper implementations across all co-located folders
        services.Scan(scan => scan
            .FromAssemblyOf<IExceptionMapper>()
            .AddClasses(classes => classes.AssignableTo<IExceptionMapper>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
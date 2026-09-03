using FluentValidation;

namespace TaskApi.Shared.Extensions;

public static class FeatureExtensions
{
    public static IServiceCollection AddFeatureInfrastructure(this IServiceCollection services)
    {
        // 1. Auto-discover all FluentValidation validators in the assembly
        services.AddValidatorsFromAssemblyContaining<Program>();

        // 2. Auto-discover and register all Feature Handlers using Scrutor
        //    Matches any class ending with "Handler" or nested inside feature slices
        services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Handler") || type.Name == "Handler"))
            .AsSelf()
            .WithScopedLifetime());

        return services;
    }
}
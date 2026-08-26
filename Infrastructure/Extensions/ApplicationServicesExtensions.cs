using FluentValidation;

namespace TaskApi.Infrastructure.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();

        // Auth
        services.AddScoped<Features.Auth.Signup.Handler>();
        services.AddScoped<Features.Auth.Login.Handler>();

        // System, Profile, Public
        services.AddScoped<Features.Profile.GetProfile.Handler>();
        services.AddScoped<Features.Public.GetPublicInfo.Handler>();

        // Tasks
        services.AddScoped<Features.Tasks.CreateTask.Handler>();
        services.AddScoped<Features.Tasks.GetTasks.Handler>();
        services.AddScoped<Features.Tasks.GetTaskById.Handler>();
        services.AddScoped<Features.Tasks.UpdateTask.Handler>();
        services.AddScoped<Features.Tasks.DeleteTask.Handler>();

        return services;
    }
}
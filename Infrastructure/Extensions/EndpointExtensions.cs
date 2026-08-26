namespace TaskApi.Infrastructure.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        // System & Public
        Features.System.GetRoot.Map(app);
        Features.System.HealthChecks.Map(app);
        Features.Public.GetPublicInfo.Map(app);

        // Auth & Profile
        Features.Auth.Signup.Map(app);
        Features.Auth.Login.Map(app);
        Features.Profile.GetProfile.Map(app);

        // Tasks
        Features.Tasks.CreateTask.Map(app);
        Features.Tasks.GetTasks.Map(app);
        Features.Tasks.GetTaskById.Map(app);
        Features.Tasks.UpdateTask.Map(app);
        Features.Tasks.DeleteTask.Map(app);

        return app;
    }

    public static WebApplication UseGlobalMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}
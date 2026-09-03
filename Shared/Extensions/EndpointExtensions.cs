using TaskApi.Shared.Interfaces;

namespace TaskApi.Shared.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        // Auto-discover all classes implementing IEndpoint across the solution
        var endpointTypes = typeof(Program).Assembly.GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) 
                     && !t.IsInterface 
                     && !t.IsAbstract);

        foreach (var type in endpointTypes)
        {
            if (Activator.CreateInstance(type) is IEndpoint endpoint)
            {
                endpoint.MapEndpoint(app);
            }
        }

        return app;
    }
}
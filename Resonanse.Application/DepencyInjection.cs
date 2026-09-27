using Microsoft.Extensions.DependencyInjection;
using Resonanse.Application.Services;

namespace Resonanse.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILibraryScanner, LibraryScanner>();
        services.AddScoped<ILibraryQueryService, LibraryQueryService>();
        services.AddScoped<IStreamService, StreamService>();
        services.AddScoped<ILibraryEditService, LibraryEditService>();
        return services;
    }
}
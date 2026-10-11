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
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInviteService, InviteService>();
        services.AddScoped<IInviteRequestService, InviteRequestService>();
        services.AddScoped<IPublicAlbumService, PublicAlbumService>();
        services.AddScoped<ICoverService, CoverService>();
        return services;
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resonanse.Application.Abstractions;
using Resonanse.Infrastructure.Auth;
using Resonanse.Infrastructure.Hashing;
using Resonanse.Infrastructure.Metadata;
using Resonanse.Infrastructure.Persistence;

namespace Resonanse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ResonanseDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ResonanseDb")));

        services.AddScoped<IResonanseDbContext>(sp => sp.GetRequiredService<ResonanseDbContext>());

        services.AddSingleton<IMetadataReader, TagLibMetadataReader>();
        services.AddSingleton<IFileHasher, Blake3FileHasher>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        return services;
    }
}
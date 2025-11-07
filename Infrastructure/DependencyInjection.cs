using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignFlow.Application.Options;
using SignFlow.Infrastructure.Email;
using SignFlow.Infrastructure.Persistence;

namespace SignFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var conn = config.GetConnectionString("Default") ?? throw new InvalidOperationException("Connection string 'Default' missing");
        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(conn));

        services.AddIdentityCore<IdentityUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
            options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();

        services.AddOptions<StripeOptions>().Bind(config.GetSection(StripeOptions.SectionName));
        services.AddOptions<StorageOptions>().Bind(config.GetSection(StorageOptions.SectionName));
        services.AddOptions<EmailOptions>().Bind(config.GetSection(EmailOptions.SectionName));

        services.AddScoped<IEmailSender, SendGridEmailSender>();
        services.AddHostedService<DevSeederHostedService>();
        return services;
    }
}

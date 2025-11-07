using Microsoft.Extensions.DependencyInjection;
using SignFlow.Application.Services;

namespace SignFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<PricingService>();
        services.AddScoped<PdfService>();
        services.AddScoped<SigningTokenService>(); // signing tokens
        return services;
    }
}

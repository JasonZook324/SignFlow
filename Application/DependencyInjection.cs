using Microsoft.Extensions.DependencyInjection;
using SignFlow.Application.Services;

namespace SignFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<PricingService>();
        services.AddScoped<PdfService>();
        services.AddScoped<SigningTokenService>();
        services.AddScoped<AuditService>();
        services.AddScoped<PaymentService>();
        services.AddScoped<CurrentOrganization>();
        services.AddScoped<ICurrentOrganization>(sp => sp.GetRequiredService<CurrentOrganization>());
        return services;
    }
}

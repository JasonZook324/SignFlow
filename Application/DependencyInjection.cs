using Microsoft.Extensions.DependencyInjection;
using SignFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using SignFlow.Application.Security;

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

        services.AddSingleton<IAuthorizationHandler, OrgMemberAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, OrgOwnerAuthorizationHandler>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("OrgMember", policy => policy.Requirements.Add(new OrgMemberRequirement()));
            options.AddPolicy("OrgOwner", policy => policy.Requirements.Add(new OrgOwnerRequirement()));
        });

        return services;
    }
}

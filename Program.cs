using SignFlow.Infrastructure;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Application;
using SignFlow.Application.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddRazorPages(options =>
{
    // Public webhook page should be anonymous and rate-limited
    options.Conventions.AllowAnonymousToPage("/Webhooks/Stripe");
    options.Conventions.AddPageRouteModelConvention("/Webhooks/Stripe", model =>
    {
        foreach (var selector in model.Selectors)
        {
            selector.EndpointMetadata.Add(new EnableRateLimitingAttribute("StripeWebhooks"));
        }
    });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Rate limiting (tight policy for webhooks)
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("StripeWebhooks", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(partitionKey: "stripe-webhooks",
            partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

var app = builder.Build();

// Ensure database exists and migrations are applied (dev convenience)
await DatabaseInitializer.InitializeAsync(app.Services, app.Configuration);

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseCurrentOrganization();

app.MapRazorPages();

app.Run();

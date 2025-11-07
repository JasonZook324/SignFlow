using SignFlow.Infrastructure;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Application;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddRazorPages();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

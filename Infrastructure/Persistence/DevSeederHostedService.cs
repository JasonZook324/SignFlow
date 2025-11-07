using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignFlow.Domain.Entities;

namespace SignFlow.Infrastructure.Persistence;

public class DevSeederHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly IHostEnvironment _env;

    public DevSeederHostedService(IServiceProvider services, IHostEnvironment env)
    {
        _services = services;
        _env = env;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment()) return;
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // If data already seeded (any org exists) skip
        if (db.Organizations.Any()) return;

        // Create a dev user
        var email = "dev@example.com";
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            await userManager.CreateAsync(user, "DevPass123!");
        }

        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Demo Agency",
            Slug = "demo-agency",
            OwnerUserId = user.Id,
            CreatedUtc = DateTime.UtcNow
        };
        db.Organizations.Add(org);

        var orgUser = new OrganizationUser
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            UserId = user.Id,
            Role = "Owner"
        };
        db.OrganizationUsers.Add(orgUser);

        var client = new Client
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            Name = "Acme Corp",
            Email = "contact@acme.test"
        };
        db.Clients.Add(client);

        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            ClientId = client.Id,
            Title = "Brand Identity Package",
            Status = ProposalStatus.Draft,
            Currency = "USD"
        };
        db.Proposals.Add(proposal);

        var items = new List<ProposalItem>
        {
            new() { Id = Guid.NewGuid(), ProposalId = proposal.Id, SortOrder = 1, Description = "Logo Design", Quantity = 1, UnitPrice = 1500m, Taxable = true },
            new() { Id = Guid.NewGuid(), ProposalId = proposal.Id, SortOrder = 2, Description = "Brand Guidelines", Quantity = 1, UnitPrice = 1200m, Taxable = true }
        };
        db.ProposalItems.AddRange(items);

        // Simple pricing calc
        proposal.Subtotal = items.Sum(i => i.Quantity * i.UnitPrice);
        proposal.TaxTotal = 0m; // no tax for demo
        proposal.DiscountTotal = 0m;
        proposal.GrandTotal = proposal.Subtotal + proposal.TaxTotal - proposal.DiscountTotal;

        await db.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

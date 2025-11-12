using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SignFlow.Domain.Entities;

namespace SignFlow.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationUser> OrganizationUsers => Set<OrganizationUser>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<ProposalItem> ProposalItems => Set<ProposalItem>();
    public DbSet<Signature> Signatures => Set<Signature>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<SigningToken> SigningTokens => Set<SigningToken>();
    public DbSet<ProposalTemplate> ProposalTemplates => Set<ProposalTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Updated composite indexes including IsDeleted for query perf
        modelBuilder.Entity<Proposal>().HasIndex(p => new { p.OrganizationId, p.Status, p.IsDeleted });
        modelBuilder.Entity<Client>().HasIndex(c => new { c.OrganizationId, c.Name, c.IsDeleted });
        modelBuilder.Entity<Payment>().HasIndex(p => p.ProposalId);
        modelBuilder.Entity<SigningToken>().HasIndex(t => t.Token).IsUnique();

        // Global query filters for soft-deletes
        modelBuilder.Entity<Client>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Proposal>().HasQueryFilter(p => !p.IsDeleted);
    }
}

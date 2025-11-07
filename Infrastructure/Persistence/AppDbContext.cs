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
    public DbSet<ProposalTemplate> ProposalTemplates => Set<ProposalTemplate>();
    public DbSet<Signature> Signatures => Set<Signature>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Proposal>().HasIndex(p => new { p.OrganizationId, p.Status });
        modelBuilder.Entity<Client>().HasIndex(c => new { c.OrganizationId, c.Name });
        modelBuilder.Entity<Payment>().HasIndex(p => p.ProposalId);
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Application.Services;
using Microsoft.EntityFrameworkCore;
using SignFlow;

[Authorize(Policy = "OrgMember")]
public class ProposalCreateModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly PricingService _pricing;
    private readonly ICurrentOrganization _org;
    public ProposalCreateModel(AppDbContext db, PricingService pricing, ICurrentOrganization org)
    {
        _db = db; _pricing = pricing; _org = org;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public (decimal Subtotal, decimal TaxTotal, decimal DiscountTotal, decimal GrandTotal) Calculation { get; set; }
    public List<Client> AvailableClients { get; set; } = new();

    public class ItemModel
    {
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1m;
        public decimal UnitPrice { get; set; } = 0m;
        public bool Taxable { get; set; } = true;
        public decimal? DiscountRate { get; set; }
    }

    public class InputModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Currency { get; set; } = "USD";
        public List<ItemModel> Items { get; set; } = new() { new ItemModel(), new ItemModel() };
        [Range(0, 100)]
        public decimal TaxRate { get; set; } = 0m;
        [Required]
        public Guid ClientId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (_org.OrganizationId == null)
        {
            TempData.Error("No organization context.");
            ModelState.AddModelError(string.Empty, "No organization context.");
            return Page();
        }
        AvailableClients = await _db.Clients.Where(c => c.OrganizationId == _org.OrganizationId).OrderBy(c => c.Name).ToListAsync();
        Calculation = _pricing.Calculate(Input.Items.Select(i => (i.Quantity, i.UnitPrice, i.Taxable, i.DiscountRate)), Input.TaxRate / 100m);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (_org.OrganizationId == null)
        {
            TempData.Error("No organization context.");
            ModelState.AddModelError(string.Empty, "No organization context.");
            return Page();
        }
        AvailableClients = await _db.Clients.Where(c => c.OrganizationId == _org.OrganizationId).OrderBy(c => c.Name).ToListAsync();
        Calculation = _pricing.Calculate(Input.Items.Select(i => (i.Quantity, i.UnitPrice, i.Taxable, i.DiscountRate)), Input.TaxRate / 100m);
        if (!ModelState.IsValid)
        {
            TempData.Error("Please correct the highlighted issues.");
            return Page();
        }

        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            OrganizationId = _org.OrganizationId.Value,
            ClientId = Input.ClientId,
            Title = Input.Title,
            Currency = Input.Currency,
            Status = ProposalStatus.Draft,
            Subtotal = Calculation.Subtotal,
            TaxTotal = Calculation.TaxTotal,
            DiscountTotal = Calculation.DiscountTotal,
            GrandTotal = Calculation.GrandTotal
        };
        _db.Proposals.Add(proposal);

        int sort = 1;
        foreach (var item in Input.Items)
        {
            _db.ProposalItems.Add(new ProposalItem
            {
                Id = Guid.NewGuid(),
                ProposalId = proposal.Id,
                SortOrder = sort++,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Taxable = item.Taxable,
                DiscountRate = item.DiscountRate
            });
        }
        await _db.SaveChangesAsync();
        TempData.Success("Proposal created.");
        return RedirectToPage("Index");
    }
}

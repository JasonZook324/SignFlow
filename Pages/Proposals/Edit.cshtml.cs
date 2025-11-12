using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Application.Services;
using SignFlow;

[Authorize(Policy = "OrgMember")]
public class ProposalEditModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly PricingService _pricing;
    private readonly ICurrentOrganization _org;
    public ProposalEditModel(AppDbContext db, PricingService pricing, ICurrentOrganization org) { _db = db; _pricing = pricing; _org = org; }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public (decimal Subtotal, decimal TaxTotal, decimal DiscountTotal, decimal GrandTotal) Calculation { get; set; }

    public class ItemModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public bool Taxable { get; set; }
        public decimal? DiscountRate { get; set; }
    }

    public class InputModel
    {
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Currency { get; set; } = "USD";
        public List<ItemModel> Items { get; set; } = new();
        [Range(0, 100)]
        public decimal TaxRate { get; set; } = 0m;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var proposal = await _db.Proposals.FirstOrDefaultAsync(p => p.Id == id);
        if (proposal == null) return NotFound();
        if (_org.OrganizationId == null || proposal.OrganizationId != _org.OrganizationId.Value) return Forbid();
        var items = await _db.ProposalItems.Where(i => i.ProposalId == id).OrderBy(i => i.SortOrder).ToListAsync();
        Input = new InputModel
        {
            Id = proposal.Id,
            Title = proposal.Title,
            Currency = proposal.Currency,
            Items = items.Select(i => new ItemModel
            {
                Id = i.Id,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Taxable = i.Taxable,
                DiscountRate = i.DiscountRate
            }).ToList(),
            TaxRate = 0m // TODO derive
        };
        Calculation = _pricing.Calculate(Input.Items.Select(i => (i.Quantity, i.UnitPrice, i.Taxable, i.DiscountRate)), Input.TaxRate / 100m);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var proposal = await _db.Proposals.FirstOrDefaultAsync(p => p.Id == Input.Id);
        if (proposal == null) return NotFound();
        if (_org.OrganizationId == null || proposal.OrganizationId != _org.OrganizationId.Value) return Forbid();
        Calculation = _pricing.Calculate(Input.Items.Select(i => (i.Quantity, i.UnitPrice, i.Taxable, i.DiscountRate)), Input.TaxRate / 100m);
        if (!ModelState.IsValid)
        {
            TempData.Error("Please correct the highlighted issues.");
            return Page();
        }
        proposal.Title = Input.Title;
        proposal.Currency = Input.Currency;
        proposal.Subtotal = Calculation.Subtotal;
        proposal.TaxTotal = Calculation.TaxTotal;
        proposal.DiscountTotal = Calculation.DiscountTotal;
        proposal.GrandTotal = Calculation.GrandTotal;

        var existingItems = await _db.ProposalItems.Where(i => i.ProposalId == proposal.Id).ToListAsync();
        _db.ProposalItems.RemoveRange(existingItems);
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
        TempData.Success("Proposal updated.");
        return RedirectToPage("Index");
    }
}

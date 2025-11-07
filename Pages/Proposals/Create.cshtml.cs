using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Application.Services;

[Authorize]
public class ProposalCreateModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly PricingService _pricing;
    public ProposalCreateModel(AppDbContext db, PricingService pricing)
    {
        _db = db; _pricing = pricing;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public (decimal Subtotal, decimal TaxTotal, decimal DiscountTotal, decimal GrandTotal) Calculation { get; set; }

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
    }

    public void OnGet()
    {
        Calculation = _pricing.Calculate(Input.Items.Select(i => (i.Quantity, i.UnitPrice, i.Taxable, i.DiscountRate)), Input.TaxRate / 100m);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Calculation = _pricing.Calculate(Input.Items.Select(i => (i.Quantity, i.UnitPrice, i.Taxable, i.DiscountRate)), Input.TaxRate / 100m);
        if (!ModelState.IsValid) return Page();

        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.Empty, // TODO: org context
            ClientId = Guid.Empty, // TODO: selected client
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
        return RedirectToPage("Index");
    }
}

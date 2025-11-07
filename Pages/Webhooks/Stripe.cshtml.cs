using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using SignFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SignFlow.Domain.Entities;
using SignFlow.Application.Services;

namespace SignFlow.Pages.Webhooks;

[IgnoreAntiforgeryToken]
public class StripeModel : PageModel
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;
    private readonly AuditService _audit;
    public StripeModel(IConfiguration config, AppDbContext db, AuditService audit)
    {
        _config = config; _db = db; _audit = audit;
    }

    [HttpPost]
    public async Task<IActionResult> OnPostAsync()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        var secret = _config["Stripe:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(secret)) return new ContentResult { StatusCode = 400, Content = "No webhook secret" };
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], secret);
        }
        catch (Exception ex)
        {
            return new ContentResult { StatusCode = 400, Content = ex.Message };
        }

        switch (stripeEvent.Type)
        {
            case Events.CheckoutSessionCompleted:
                if (stripeEvent.Data.Object is Session session)
                    await HandleCheckoutCompletedAsync(session);
                break;
            case Events.PaymentIntentSucceeded:
                if (stripeEvent.Data.Object is PaymentIntent intent)
                    await HandlePaymentIntentSucceededAsync(intent);
                break;
        }

        return new ContentResult { StatusCode = 200, Content = "ok" };
    }

    private async Task HandleCheckoutCompletedAsync(Session session)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.StripeCheckoutSessionId == session.Id);
        if (payment == null) return;
        payment.StripePaymentIntentId = session.PaymentIntentId ?? payment.StripePaymentIntentId;
        payment.ReceiptUrl = session.Invoice?.HostedInvoiceUrl; // may be null for one-off
        if (payment.Status != PaymentStatus.Succeeded)
        {
            payment.Status = PaymentStatus.Succeeded;
            payment.PaidUtc = DateTime.UtcNow;
            var proposal = await _db.Proposals.FirstOrDefaultAsync(pr => pr.Id == payment.ProposalId);
            if (proposal != null)
            {
                proposal.Status = ProposalStatus.Paid;
                proposal.PaidUtc = DateTime.UtcNow;
                await _audit.WriteAsync(proposal.OrganizationId, nameof(Proposal), proposal.Id, "PaymentSucceeded", new
                {
                    payment.Amount,
                    payment.Currency,
                    session.Id,
                    payment.StripePaymentIntentId
                });
            }
            await _db.SaveChangesAsync();
        }
    }

    private async Task HandlePaymentIntentSucceededAsync(PaymentIntent intent)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id);
        if (payment == null) return;
        if (payment.Status != PaymentStatus.Succeeded)
        {
            payment.Status = PaymentStatus.Succeeded;
            payment.PaidUtc = DateTime.UtcNow;
            // ReceiptUrl omitted (requires expanded charge retrieval)
            var proposal = await _db.Proposals.FirstOrDefaultAsync(pr => pr.Id == payment.ProposalId);
            if (proposal != null)
            {
                proposal.Status = ProposalStatus.Paid;
                proposal.PaidUtc = DateTime.UtcNow;
                await _audit.WriteAsync(proposal.OrganizationId, nameof(Proposal), proposal.Id, "PaymentSucceeded", new
                {
                    payment.Amount,
                    payment.Currency,
                    intent.Id,
                    payment.StripeCheckoutSessionId
                });
            }
            await _db.SaveChangesAsync();
        }
    }
}

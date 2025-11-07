using Stripe;
using Stripe.Checkout;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SignFlow.Application.Services; // for AuditService

namespace SignFlow.Application.Services;

public class PaymentService
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;
    public PaymentService(AppDbContext db, AuditService audit)
    {
        _db = db; _audit = audit;
    }

    public async Task<Session> CreateCheckoutSessionAsync(Proposal proposal, string successUrl, string cancelUrl)
    {
        StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("Stripe__ApiKey") ?? string.Empty;

        // Prevent duplicate succeeded payment
        var hasPayment = await _db.Payments.AnyAsync(p => p.ProposalId == proposal.Id && p.Status == PaymentStatus.Succeeded);
        if (hasPayment) throw new InvalidOperationException("Payment already completed for this proposal.");

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = proposal.Currency.ToLowerInvariant(),
                        UnitAmountDecimal = (long)(proposal.GrandTotal * 100m),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = proposal.Title
                        }
                    },
                    Quantity = 1
                }
            }
        };
        var service = new SessionService();
        var session = await service.CreateAsync(options);

        // Audit checkout creation
        await _audit.WriteAsync(proposal.OrganizationId, nameof(Proposal), proposal.Id, "CheckoutCreated", new
        {
            session.Id,
            proposal.GrandTotal,
            proposal.Currency
        });

        _db.Payments.Add(new Payment
        {
            Id = Guid.NewGuid(),
            ProposalId = proposal.Id,
            Amount = proposal.GrandTotal,
            Currency = proposal.Currency,
            Status = PaymentStatus.Pending,
            StripeCheckoutSessionId = session.Id,
            StripePaymentIntentId = session.PaymentIntentId ?? string.Empty
        });
        await _db.SaveChangesAsync();
        return session;
    }
}

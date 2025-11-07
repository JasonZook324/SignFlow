# Implementation Plan (Razor Pages .NET 9 Micro-SaaS: Proposals, E‑Signature, Payments)

## 1. Product Scope (MVP -> Expansion)
MVP (Weeks 1–3)
- Auth & Org: Email/password (ASP.NET Core Identity), Organization membership (Owner, Member)
- Core Entities: User, Organization, Client, Proposal, ProposalItem, ProposalTemplate, Signature, Payment, Subscription, AuditEvent
- Proposal Lifecycle: Draft -> Sent -> Viewed -> Signed -> Paid -> Archived
- Proposal Builder: Sections, line items (qty, unit price, tax/discount flags)
- PDF Generation: Server-side (QuestPDF) with embedded signature + audit sheet
- E‑Signature: Single external signer, public token link `/Sign/{token}`, capture PNG + vector points, IP, timestamp
- Payments: Stripe Checkout (one-off), deposit % optional
- Billing: Stripe Billing for subscription tiers (Starter/Pro) + webhook ingestion
- Notifications: Email (SendGrid) for proposal sent, viewed, signed, payment received
- Dashboard: Counts (drafts, sent, signed, payments), recent activity feed
- Security: Role-based authorization, anti-forgery, secure public token (Guid + short slug)
- Observability: Application Insights (requests, exceptions, custom events)

Phase 2 (Weeks 4–8)
- Multi-party signing (sequence + parallel)
- Proposal Templates library + variables (e.g. {{ClientName}})
- Client Portal `/ClientPortal/{publicId}` (proposals, payments history)
- Internal comments & activity log
- Reminders & expirations (background job scheduler)
- Advanced pricing (option groups, upsells)
- Basic analytics (open rate, time-to-sign, win ratio)
- ACH payments + saved payment methods (Stripe Payment Element)
- Webhooks retry + dead-letter queue

Phase 3 (Weeks 9–12)
- White-label & custom domain (per organization)
- API + Webhooks for external automation
- Integrations (QuickBooks invoice push, HubSpot deal sync)
- SSO (OIDC) for Business tier
- Advanced e-sign fields (date, text input, initials)
- Proposal variants (A/B)
- AI Assist (content generation via external provider, isolated service)
- Role granularity + audit exports (CSV)

## 2. High-Level Architecture
- Presentation: Razor Pages (.NET 9), minimal JS (vanilla + unobtrusive validation), SignalR (later for presence/chat)
- Application Layer: Services (ProposalService, SignatureService, BillingService, PdfService, NotificationService)
- Domain Model: Plain C# classes + EF Core value objects (Money, TaxRate, SignatureData)
- Persistence: EF Core 9 (SQL Server or PostgreSQL), migrations per feature module
- Files/Assets: Azure Blob Storage (PDFs, logos, signature images), stored with path conventions
- Background Processing: HostedService (initial) -> Hangfire/Azure WebJobs (scale)
- Integration Adapters: Stripe, SendGrid, QuestPDF, (future) QuickBooks, HubSpot
- Security: Identity + cookie auth, tokenized public endpoints, Data Protection (key ring in blob/storage)
- Observability: Application Insights telemetry + custom events (ProposalViewed, ProposalSigned)
- Configuration: Strongly typed options classes (StripeOptions, StorageOptions)

## 3. Data Model (Initial Tables)
- Users(Id, Email, PasswordHash, DisplayName, CreatedUtc)
- Organizations(Id, Name, Slug, OwnerUserId, CreatedUtc)
- OrganizationUsers(Id, OrganizationId, UserId, Role)
- Clients(Id, OrganizationId, Name, Email, Phone, ExternalRef)
- Proposals(Id, OrganizationId, ClientId, Status, Title, Currency, Subtotal, TaxTotal, DiscountTotal, GrandTotal, SentUtc, SignedUtc, PaidUtc, ExpiresUtc)
- ProposalItems(Id, ProposalId, SortOrder, Description, Quantity, UnitPrice, Taxable, DiscountRate)
- ProposalTemplates(Id, OrganizationId, Name, JsonDefinition)
- Signatures(Id, ProposalId, SignerName, SignerEmail, SignedUtc, IP, UserAgent, ImagePath, VectorJson, Hash)
- Payments(Id, ProposalId, StripePaymentIntentId, Amount, Currency, Status, PaidUtc, Method)
- Subscriptions(Id, OrganizationId, StripeSubscriptionId, PlanCode, Status, CurrentPeriodEndUtc)
- AuditEvents(Id, OrganizationId, EntityType, EntityId, EventType, DataJson, CreatedUtc)

## 4. Key Razor Pages (MVP)
- /Index (Marketing or redirect if authenticated)
- /Dashboard
- /Clients (List/Create/Edit)
- /Proposals (List/Create/Edit/Send)
- /Proposals/{id} (View internal + actions)
- /Proposals/{id}/Pdf (Download)
- /Sign/{token} (Public signing surface)
- /Billing (Manage subscription, usage, invoices)
- /Webhooks/Stripe (POST endpoint)
- /Account/* (Identity scaffold)
- /Settings/Organization

## 5. Security & Compliance
- Input validation (FluentValidation optional)
- Authorization policies: OrgMembership, OrgOwner
- Public tokens: one-time signing link rotates after use
- Data at rest: encrypted at DB-level (optional later: transparent encryption of signature blob)
- Tamper-evident PDFs: store SHA256 hash of binary post-sealing
- Audit trail: append events for view/sign actions
- Rate limiting: ASP.NET Core built-in middleware for public endpoints
- Secrets: User Secrets (dev) -> Azure Key Vault (prod)

## 6. Performance Considerations
- Lean EF queries (projection DTOs for lists)
- Caching: proposal PDF hash + CDN headers for static assets
- Async IO for storage & Stripe calls
- Deferred heavy work (email sending, PDF sealing) to background jobs
- Indexes: Proposal(Status, OrganizationId), Client(OrganizationId, Name), Payments(ProposalId)

## 7. Testing Strategy
- Unit: Services (pricing calc, signature hashing, tax computation)
- Integration: Razor Page handlers (TestServer), Stripe webhook processing
- End-to-end: Playwright (signing flow, payment success)
- Load: k6 script for concurrent signing link hits
- Security: automated OWASP dependency scan + basic ZAP baseline

## 8. CI/CD Pipeline
Stages:
1. Restore & Build (__Build__)
2. Run Tests (__Test Explorer__ integration via command line)
3. Publish Artifacts (dotnet publish -c Release)
4. Static Analysis (Roslyn analyzers, optional SonarCloud)
5. Containerize (Dockerfile, multi-stage)
6. Deploy Dev (Azure Web App / Container App)
7. Run Smoke Tests (health + simple proposal creation)
8. Manual Approval -> Prod
9. Deploy Prod + Run migrations
10. Tag release + push changelog

## 9. Migration & Environment Flow
Environments: Local -> Dev -> Staging -> Prod
- Migrations auto-run in Dev/Staging
- Manual gated migration in Prod (script + backup snapshot)
- Feature flags for phased launches (appsettings + DB overrides)

## 10. Telemetry & Metrics
Business Metrics:
- Proposal creation/week
- View -> Sign conversion %
- Avg time-to-sign (median)
- Payment completion rate
- MRR, churn, ARPU
Technical Metrics:
- Request latency (p95)
- PDF generation duration
- Webhook processing success rate
- Background job queue depth

## 11. Backlog (Prioritized, MVP)
1. Identity + Org scaffolding
2. Core data model + EF migrations
3. Proposal CRUD + pricing calc
4. PDF generation
5. Signing flow (token, capture, audit)
6. Stripe one-off payments
7. Subscription + Stripe Billing webhook
8. Dashboard + activity feed
9. Email notifications
10. Basic analytics events
11. Hardening (rate limit, logging)
12. Documentation (README + /Help)

## 12. 30 / 60 / 90 Day Milestones
Day 30: MVP live, first paying test orgs, signing/payment functional, baseline metrics
Day 60: Multi-party signing, client portal, reminders, ACH, enhanced analytics
Day 90: White-label, API/webhooks, SSO (beta), advanced fields, integration pilot

## 13. Risk & Mitigation
- Payment delays: rely on Stripe webhooks + reconciliation job
- PDF integrity: hash + store; re-generate only pre-sign
- Multi-currency complexity: localize money formatting early
- Scaling background jobs: plan migration to dedicated queue (Azure Storage Queue + worker)

## 14. Documentation & Developer Experience
- /Docs (markdown) for API endpoints, signature model, pricing logic
- Postman collection for webhooks + API (later)
- README sections: Architecture, Setup, Deployment, Observability

## 15. Decomposition for Parallel Work
Track slices: Auth/Org, Proposal Engine, PDF & Signature, Billing/Stripe, Notifications, Dashboard & Analytics, Infrastructure (CI/CD), Portal & Reminders

(End)
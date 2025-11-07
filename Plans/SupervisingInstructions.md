# Supervising Instructions for SignFlow Implementation

Provide these instructions one at a time. After each instruction is executed I will summarize changes, run a build/tests as relevant, report issues, and wait for further direction.

1) Read the plans
"Read `Plans/ImplementationPlan.md` and `Plans/BuildPlan.md`. Summarize scope and produce a prioritized 10–15 item engineering backlog mapped to concrete pages/services."

2) Audit the repo and reconcile with plans
"List all projects and key files. Identify gaps vs the MVP plan and propose the exact directories/files to add to align with Razor Pages."

3) Prepare solution structure
"Create `Pages`, `Domain`, `Application`, `Infrastructure` folders and skeleton files to match the architecture. Propose namespaces and DI registrations."

4) Add core packages
"Add EF Core, Identity, configuration options, PDF (QuestPDF), Stripe, and email packages. Update the project file and restore."

5) App configuration
"Create strongly-typed options (`StripeOptions`, `StorageOptions`). Add `appsettings.Development.json` placeholders and wire options binding and validation."

6) Identity and organization model
"Integrate ASP.NET Core Identity. Add `Organization` and `OrganizationUser` entities, policies (`OrgMembership`, `OrgOwner`), and minimal org selection middleware."

7) Data model and DbContext
"Implement entities from Section 3 of the plan, the `AppDbContext`, configurations, and indexes. Generate and apply the initial migration."

8) Seed data (Dev only)
"Add a development seeder to create a sample org, user, client, proposal. Make it conditional on environment."

9) Razor Pages scaffolding
"Scaffold Razor Pages for `/Dashboard`, `/Clients` (List/Create/Edit), `/Proposals` (List/Create/Edit/Send/View), `/Proposals/{id}/Pdf`, `/Sign/{token}`, `/Billing`, `/Webhooks/Stripe`, `/Settings/Organization` with handlers and minimal markup."

10) Pricing calculation service
"Implement a `PricingService` (subtotal, tax, discounts). Add unit tests covering edge cases and money rounding."

11) PDF generation
"Add `PdfService` using QuestPDF. Implement proposal rendering and an audit sheet. Wire `/Proposals/{id}/Pdf` to return a file result."

12) Signing flow
"Create secure public signing tokens, the `/Sign/{token}` page, signature capture (canvas ? PNG), storage, and the audit log append. Update proposal status transitions."

13) Payments (one-off)
"Integrate Stripe Checkout for proposal payment with optional deposit. Add success/cancel routes and persist `Payment` records."

14) Stripe webhooks
"Implement `/Webhooks/Stripe` to handle key events (payment_intent.succeeded, checkout.session.completed). Make it idempotent and secure."

15) Subscription billing
"Integrate Stripe Billing for Starter/Pro. Add `/Billing` page to view plan, invoices, and payment method. Handle subscription updated/canceled webhooks."

16) Notifications
"Add email sender (SendGrid). Implement notifications for sent/viewed/signed/paid events. Queue sending in a background hosted service."

17) Dashboard and analytics
"Implement dashboard summaries and simple analytics (view-to-sign conversion, recent activity). Add telemetry events."

18) Security hardening
"Enable HTTPS/HSTS, anti-forgery on POSTs, rate limiting for public endpoints, CSP headers, and input sanitization for rich text."

19) Tests and quality gates
"Add integration tests for Razor Page handlers and webhook processing. Ensure build passes with analyzers and warnings-as-errors."

20) CI/CD setup
"Create pipeline config to restore, build, test, publish, and containerize. Add a dev deploy job and smoke tests."

21) Containerization and deployment
"Add a multi-stage Dockerfile and health endpoints. Provide Azure Web App/Container App deployment settings."

22) Documentation
"Add `/Docs` with setup, environment config, and runbooks. Update README with architecture and local run instructions."

23) Feature flags
"Introduce flags for multi-party signing, client portal, and white-label. Gate pages/features accordingly."

24) Final hardening pass
"Run a full build, apply migrations, execute tests, and produce a punch list of remaining defects and polish items."

25) Release preparation
"Create a changelog entry, tag a release, and produce a minimal onboarding guide for first users."

Process Expectations:
- After each instruction I will: execute changes, run build (and tests when present), list modified/created files, surface errors, request next instruction.
- I will not proceed to the next step without your explicit instruction.

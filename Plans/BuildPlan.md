# Build & Delivery Plan

## 1. Tooling & Stack
- .NET 9, Razor Pages, EF Core 9, Application Insights
- Libraries: QuestPDF, Stripe.NET, SendGrid, FluentValidation (optional), HtmlSanitizer (for rich text sections)
- Dev Environment: VS 2022 (__Manage NuGet Packages__), Docker Desktop, az CLI
- Branch Strategy: main (prod), develop (integration), feature/* (short-lived)
- Versioning: Semantic (v0.x for MVP, v1.0 at GA)
- Code Quality: Nullable enabled, analyzers (Microsoft + StyleCop), treat warnings as errors (Release)

## 2. Project Structure
- /src/Web (Razor Pages, static assets, DI wiring)
- /src/Core (Domain models, interfaces)
- /src/Infrastructure (EF Core, storage adapters, Stripe adapter)
- /src/Application (Services, validators)
- /tests/UnitTests
- /tests/IntegrationTests
- /Plans (living documents)

## 3. Initial Setup Tasks
- Enable nullable, analyzers
- Configure EF DbContext + migrations
- Add Identity scaffolding (minimal fields)
- Set up Dependency Injection (services by layer)
- Add Application Insights instrumentation
- Stripe & SendGrid API keys via User Secrets (local) -> Azure Key Vault (prod)

## 4. Incremental Build Sequence (MVP)
Sprint 1 (Week 1)
- Auth & Org join/invite flow
- Core entities & migrations
- Proposal CRUD (basic form)
- Pricing calculation service
- PDF prototype (static sample)

Sprint 2 (Week 2)
- Dynamic PDF assembly (items, totals)
- Signing token generator + public page
- Signature capture (Canvas -> PNG)
- Audit logging (view/sign)
- Stripe one-off payment (Checkout session)
- Payment confirmation + status update

Sprint 3 (Week 3)
- Subscription plans (Stripe Billing integration)
- Webhook endpoint + event handlers (payment_intent.succeeded, customer.subscription.updated)
- Dashboard summary queries
- Email notifications pipeline (queued background tasks)
- Basic analytics events + ingestion
- Hardening (rate limiting, error pages, logging enrichment)

## 5. Build Automation (CI)
GitHub Actions / Azure DevOps Pipeline:
Jobs:
- restore: dotnet restore
- build: dotnet build --configuration Release /warnaserror
- test: dotnet test --collect:"XPlat Code Coverage"
- analyze: run dotnet format --verify-no-changes, optional Sonar scan
- publish: dotnet publish -c Release -o artifact
- docker: build & push image (tags: sha + semver)
- deploy-dev: az webapp deployment or Container App revision
- smoke: run minimal Playwright script (proposal create + sign stub)
- approval (manual)
- deploy-prod

## 6. Local Developer Workflow
1. git pull develop
2. dotnet build
3. dotnet ef database update
4. Run Web project (F5 / __Start Debugging__)
5. Exercise pages; check logs in __Output__ (Build/Debug)

## 7. Environments & Config
Configuration sources precedence:
1. appsettings.json
2. appsettings.{Environment}.json
3. User Secrets (Development)
4. Environment Variables (CI/CD)
5. Azure Key Vault (Production)

Feature Flags (appsettings + DB override):
- MultiPartySigningEnabled
- ClientPortalEnabled
- WhiteLabelEnabled

## 8. Deployment Strategy
- Containerized (Linux) for portability
- Zero-downtime: slot swap (Staging -> Production)
- DB migrations: run pre-swap; verify success; abort on failure
- Rollback: previous slot image + point-in-time database restore if schema breaking

## 9. Quality Gates
- All tests pass (≥80% critical service coverage)
- No critical analyzer warnings
- Security dependency scan clean (GitHub Dependabot)
- Manual review: sensitive endpoints, token exposure
- Load test (k6) baseline before GA (p95 < 500ms signing page)

## 10. Testing Layers
Unit:
- Pricing, tax, discount, signature hash generation
Integration:
- Webhook event -> state transitions
- Razor Page handlers permission checks
E2E (Playwright):
- Create proposal -> send -> sign -> payment -> dashboard update
Regression Suite:
- Run nightly against staging

## 11. Logging & Monitoring
Serilog (structured):
- CorrelationId per request (middleware)
- Enrich with UserId, OrgId
Dashboards:
- Request volume, failure rate
- Proposal funnel (custom events)
Alerts:
- High error rate (>2% 5-min window)
- Webhook failures >10 in 10 min
- Background queue depth > 100 pending

## 12. Data Seeding
Dev:
- Demo org + sample proposals
- Stripe test keys + mock webhooks (CLI trigger)
Prod:
- No sample data; first user creates org
Seeding mechanism: IHostedService runs once (environment check)

## 13. Security Review Checklist Before GA
- HTTPS enforcement
- HSTS header
- Anti-forgery on internal POST handlers
- Content Security Policy (script-src 'self')
- Input sanitization for rich text sections
- Token entropy (≥128 bits)
- Proper error handling (no stack trace leakage)

## 14. Release Management
- Release Candidate branch (rc)
- Changelog generated from conventional commits
- Tag: v0.9.0-rc1 -> v1.0.0
- Post-release tasks: analytics baseline snapshot, performance bench, DB schema documentation

## 15. Continual Improvement Backlog
- Incremental static caching for PDFs
- Background heartbeat monitor
- Test data builder utilities
- Parallel test execution optimization
- Benchmark critical pricing service

(End)
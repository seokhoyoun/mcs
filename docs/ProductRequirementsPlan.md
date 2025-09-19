# Product Requirements Plan: Nexus

## Document Control
- Author: Seokho Youn
- Version: 0.1 (Draft)
- Last Updated: 2025-02-14
- Status: In progress

## Product Overview
### Vision
Enable operations teams to orchestrate, monitor, and optimize automated material flows across semiconductor and advanced manufacturing facilities from a single, responsive control plane.

### Objectives
- Deliver a unified command center for areas, locations, lots, transports, and stockers.
- Provide orchestration logic that automates plan execution while preserving operator overrides.
- Expose real-time visibility through a Blazor Server UI backed by event-driven updates.
- Offer extensible integration surfaces for third-party automation and analytics platforms.

### Success Metrics
- Achieve 99.5 percent orchestrator availability across rolling 30-day windows.

## Strategic Alignment
- Supports the broader initiative to modernize factory automation tooling and consolidate legacy schedulers.
- Leverages the existing Redis and PostgreSQL investments to keep infrastructure costs predictable.
- Aligns with the roadmap to expand into warehouse and logistics domains by providing modular domain models.

## Stakeholders & Personas
- **Production Supervisor**: Requires real-time dashboards and the ability to trigger or pause plans.
- **Automation Engineer**: Configures orchestration rules, integrates external systems, and monitors workflows.
- **Operations Analyst**: Reviews historical telemetry to optimize throughput and plan configurations.
- **IT Administrator**: Manages deployment, user access, and infrastructure health.

## Assumptions
- Core domain entities (areas, locations, lots, transports, stockers) are already modeled within Nexus.Core.
- Event-driven updates rely on Redis Pub/Sub channels mediated by Nexus.Infrastructure.
- PostgreSQL (via Supabase) persists long-lived state and audit trails.
- Operators access the system primarily through the Blazor Server Portal.

## Constraints
- C# 12 on .NET 8 is the standard for all services and libraries.
- All configuration secrets remain outside of source control and rely on environment variables or user secrets.
- System must remain deployable via Docker Compose for parity between development and production-like environments.
- Network latency between Redis, PostgreSQL, and service containers must stay below 50 ms round trip within the target deployment topology.

## Functional Requirements
- Present a real-time operations dashboard in Nexus.Portal with area, location, and lot status summaries.
- Allow supervisors to trigger, pause, or resume plans via orchestrator APIs exposed by Nexus.Gateway.
- Ingest external events (e.g., equipment status updates) through the gateway and push them into Redis channels for orchestration.
- Provide configuration screens for automation engineers to define plans, routing rules, and exception handling policies.
- Persist all orchestration decisions and outcomes to PostgreSQL for auditing and replay.
- Notify connected clients of critical events (plan failures, lots in jeopardy) within five seconds via SignalR or equivalent real-time mechanism.
- Support role-based access controls with clear separation between operators, engineers, and administrators.

## Non-Functional Requirements
- **Performance**: Median dashboard refresh latency below two seconds; orchestrator command execution acknowledged within 500 ms.
- **Scalability**: Horizontal scaling through additional orchestrator and portal instances under Docker Compose and future Kubernetes deployments.
- **Reliability**: Graceful degradation when Redis is unavailable, including queued retries and operator alerts.
- **Security**: Enforce TLS for external ingress, hash sensitive credentials, and audit privileged actions.
- **Usability**: Provide contextual tooltips and activity logs so operators can understand system state without switching tools.

## Technical Requirements
- Utilize Nexus.Infrastructure abstractions to avoid direct Redis or PostgreSQL coupling in higher-level modules.
- Maintain consistent domain events and contracts across services via shared types in Nexus.Shared.
- Ensure all projects target net8.0 with nullable reference types enabled and explicit local variable types (no var).
- Include automated regression tests in tests/Nexus.UnitTest and integration pipelines in tests/Nexus.IntegrationTest for critical workflows.

## User Experience & Flows
- Default landing page displays a global operations map with drill-down into specific areas and locations.
- Command panel allows supervisors to issue plan directives with validation and preview of downstream effects.
- Event timeline surfaces o
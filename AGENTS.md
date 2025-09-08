# Repository Guidelines

## Project Structure & Module Organization
- `src/apps`: Executables — `Nexus.Gateway`, `Nexus.Orchestrator`, `Nexus.Portal`.
- `src/libs`: Shared libraries — `Nexus.Core`, `Nexus.Infrastructure`, `Nexus.Shared`.
- `src/data`: Seed/config data (e.g., `areas.json`, `locations.csv`).
- `tests`: `Nexus.UnitTest`, `Nexus.IntegrationTest`, `Nexus.Sandbox`.
- `artifacts`: Build outputs (configured via `Directory.build.props`).
- Root: `Nexus.sln`, Docker files (`docker-compose.yml`).

## Build, Test, and Development Commands
- Restore/build: `dotnet restore` then `dotnet build Nexus.sln -c Debug`.
- Run apps locally:
  - `dotnet run --project src/apps/Nexus.Portal` (UI)
  - `dotnet run --project src/apps/Nexus.Gateway`
  - `dotnet run --project src/apps/Nexus.Orchestrator`
- Compose stack: `docker compose up -d` (Redis, Postgres, n8n, Prometheus, Loki, apps).
- Tests: `dotnet test -c Debug` (coverage: `dotnet test --collect:"XPlat Code Coverage"`).

## Coding Style & Naming Conventions
- C# 12, `net8.0`, nullable enabled; implicit usings on.
- Indentation: 4 spaces; UTF-8; one class per file.
- Naming: PascalCase (types/methods), camelCase (locals/params), `I`-prefix for interfaces, `Async` suffix for async.
- Projects/folders use PascalCase; namespaces mirror folder structure.
- Formatting: prefer `dotnet format` before PRs.

 - Do not use `var` for local declarations; always use explicit types.
 - Do not use the null-coalescing operators `??` or `??=`; prefer explicit null checks or conditional expressions.
 - Avoid ternary (`?:`) conditional operator in favor of clear `if/else` statements unless it materially improves readability.

## Testing Guidelines
- Framework: xUnit (`Fact`/`Theory`); coverage via Coverlet collector.
- Test naming: `ClassName_MethodUnderTest_ExpectedBehavior`.
- File naming: `*Tests.cs` grouped by target area (e.g., `Core/…`).
- Run all tests from repo root with `dotnet test`.

## Commit & Pull Request Guidelines
- History shows short, imperative messages (no strict convention). Keep messages concise and descriptive; consider scopes: `portal:`, `gateway:`, `orchestrator:`.
- PRs: clear description, linked issues (`#123`), steps to validate, and screenshots/GIFs for UI changes.
- Ensure builds/tests pass and Docker changes include updated compose notes if applicable.

## Security & Configuration Tips
- Config via `appsettings.json` and `appsettings.Development.json`; avoid committing secrets.
- Local secrets: use User Secrets for apps with `UserSecretsId`.
- Key env vars: `Redis__ConnectionString`, `ASPNETCORE_ENVIRONMENT`.
- Ports: Portal `8080`, Gateway `8082`, Orchestrator `8081`, Prometheus `9090`, Grafana `3000`.

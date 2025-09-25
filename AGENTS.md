# Repository Guidelines

## Agent Communication
- 모든 에이전트 응답은 한국어로 작성한다.

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
- Do not use tuples (ValueTuple or tuple literals) for return types or locals; avoid tuple deconstruction. Prefer dedicated types, records, or out parameters for multiple values.

## Testing Guidelines
- Framework: xUnit (`Fact`/`Theory`); coverage via Coverlet collector.
- Test naming: `ClassName_MethodUnderTest_ExpectedBehavior`.
- File naming: `*Tests.cs` grouped by target area (e.g., `Core/...`).
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

## Documentation References
- Product Requirements Plan: docs/ProductRequirementsPlan.md
- Product Requirements Document: docs/ProductRequirementsDocument.md

## Portal UI (MudBlazor 8.x) 주의사항
- Nexus.Portal은 MudBlazor 8.x(현재 csproj 기준 8.12.0)를 사용한다.
- 입력 컴포넌트 바인딩은 `@bind-Value`를 기본으로 사용한다.
  - 예) `MudSwitch`는 `@bind-Value`를 사용한다. `@bind-Checked`는 동작하지 않는다.
- 제네릭 타입을 명시한다.
  - 예) `MudRadioGroup T="string"`, `MudNumericField T="double"` 등.
- `MudRadioGroup` 바인딩 시 타입 유추 문제가 발생할 수 있으므로, 필요 시 이벤트를 명시한다.
  - 예) `SelectedOption="@state" SelectedOptionChanged="@((string v) => OnChanged(v))"` 형태로 사용.
- MudBlazor Analyzer 경고(MUD000x)가 보이면 먼저 속성명이 해당 버전에서 유효한지 확인하고, 가능한 최신 API(`Value/ValueChanged/ValueExpression`, `SelectedOption/SelectedOptionChanged`) 패턴을 따른다.

## 한글 인코딩 주의
- 파일 인코딩: 모든 소스/문서는 UTF-8(BOM 없음)로 저장한다.
- Windows 터미널 출력 깨짐 대응:
  - PowerShell: `[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; $OutputEncoding = [System.Text.UTF8Encoding]::new()` 실행.
  - Command Prompt(cmd): `chcp 65001`로 코드 페이지를 UTF-8로 변경.
  - 터미널 폰트는 CJK 지원 폰트(예: Cascadia Code PL, D2Coding) 사용.
- .NET 콘솔 앱에서 한글 깨짐 시 `Program.cs` 초기에 다음을 설정한다:
  - `using System.Text;`
  - `Console.OutputEncoding = Encoding.UTF8; Console.InputEncoding = Encoding.UTF8;`
- Git 출력/로그 인코딩 권장 설정:
  - `git config --global core.quotepath false` (한글 파일명 이스케이프 방지)
  - `git config --global i18n.commitEncoding utf-8`
  - `git config --global i18n.logOutputEncoding utf-8`
- VS Code 권장 설정: `"files.encoding": "utf8"`, 터미널 프로필은 PowerShell 사용 권장.
- Docker/리눅스 환경: 로케일을 `C.UTF-8` 또는 `en_US.UTF-8`로 설정하고, 로그 수집기에서 UTF-8을 사용한다.


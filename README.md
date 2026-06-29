# BattleReady

A Pathfinder 2e combat calculator that computes expected damage per round across multiple attacks.
Built as a portfolio project to demonstrate modern .NET development practices including clean architecture, dependency inversion, Entity Framework Core, REST API design, JWT authentication, structured logging, API versioning, rate limiting, server-side caching, request correlation, custom domain exception handling, and both unit and integration testing.

---

## What It Does

BattleReady models a full attack sequence and calculates **expected damage per round** by accounting for:

- **Multiple Attack Penalty (MAP)** — standard (−5/−10) or agile (−4/−8)
- **Degrees of success** — Critical Hit, Hit, Miss, and Critical Miss, each with configurable damage
- **Natural 20 upgrades** and **Natural 1 downgrades** (toggleable)
- **Dice-based damage expressions** — e.g. `2d6+3 slashing`, `d8`, `5 fire`
- **Saving throw spells** — auto-applies half damage on a Miss and zero on a Critical Miss by default
- **Default attack templates** — define a base attack once and reuse it across an entire sequence

All API requests are logged to an Azure SQL Database, with a queryable log endpoint supporting filtering and pagination.

---

## Live Demo

Swagger UI: [https://battleready-api-b4h4brhga5dea5ay.westus3-01.azurewebsites.net/swagger](https://battleready-api-b4h4brhga5dea5ay.westus3-01.azurewebsites.net/swagger)

Health check: [https://battleready-api-b4h4brhga5dea5ay.westus3-01.azurewebsites.net/health](https://battleready-api-b4h4brhga5dea5ay.westus3-01.azurewebsites.net/health)

> **Note:** The API is hosted on Azure's Free F1 tier and may take 30–60 seconds to wake up after a period of inactivity.

---

## API Endpoints

All endpoints are versioned under `/api/v1/` or `/api/v2/`. See [API Versioning](#api-versioning) below.

### `POST /api/v1/Auth/token`

Returns a signed JWT for use with protected endpoints. Submit credentials in the request body; the token is valid for 60 minutes.

**Example request:**

```json
{
  "username": "battleready",
  "password": "password123"
}
```

**Example response:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

Include the returned token in the `Authorization` header of subsequent requests to protected endpoints:

```
Authorization: Bearer <token>
```

---

### `POST /api/v1/Calculator/calculate`

The primary endpoint. Submits a full attack sequence and receives per-attack breakdowns plus a grand total. Logs each request to the database.

**Example request — using a default attack template:**

```json
{
  "enemyDefense": 19,
  "characterName": "Corrupted Wildfire",
  "natural20Upgrades": true,
  "natural1Downgrades": true,
  "defaultAttack": {
    "baseToHit": 12,
    "normalHitDamage": "1d6+6 fire",
    "critHitDamage": "dbl",
    "normalMissDamage": "0",
    "critMissDamage": "0"
  },
  "attacks": [
    { "attackNumber": 1, "isDefaultAttack": true },
    { "attackNumber": 2, "isDefaultAttack": true },
    { "attackNumber": 3, "isDefaultAttack": true }
  ]
}
```

The `defaultAttack` object acts as a template. Concrete attacks with `"isDefaultAttack": true` inherit all fields from it; their own `attackNumber` is preserved. `defaultAttack` uses a separate type (`DefaultAttackRequest`) — it has no `attackNumber` or `isDefaultAttack` field, and `normalHitDamage` is required on it unconditionally.

**Example request — explicit attacks without a template:**

```json
{
  "enemyDefense": 19,
  "natural20Upgrades": true,
  "natural1Downgrades": true,
  "attacks": [
    {
      "attackNumber": 1,
      "baseToHit": 12,
      "normalHitDamage": "1d6+6 slashing",
      "critHitDamage": "dbl",
      "normalMissDamage": "0",
      "critMissDamage": "0"
    }
  ]
}
```

**Example response (abbreviated):**

```json
{
  "attackResponses": [
    {
      "attackNumber": 1,
      "effectiveSkillRating": 12,
      "effectiveTargetScore": 19,
      "critHitChance": 0.2,
      "normalHitChance": 0.5,
      "normalMissChance": 0.25,
      "critMissChance": 0.05,
      "avgDmgCritHit": 19.0,
      "avgDmgNormalHit": 9.5,
      "avgDmgNormalMiss": 0.0,
      "avgDmgCritMiss": 0.0,
      "totalExpectedDamage": 8.55
    }
  ],
  "totalExpectedDamageAllAttacks": 8.55,
  "calculatedAt": "2026-06-29T00:00:00Z"
}
```

---

### `POST /api/v1/HitChance/calculate`

Calculates hit chance breakdown for a single attack roll. Logs each request to the database.

### `GET /api/v1/HitChance/calculate`

Same calculation as the POST version but read-only — no logging. Parameters are passed as query string values, making results bookmarkable and cacheable. Results are served from server-side memory cache when the same inputs are repeated.

**Example:** `/api/v1/HitChance/calculate?toHit=12&defense=19&natural20Upgrades=true&natural1Downgrades=true`

**Example response:**

```json
{
  "toHit": 12,
  "defense": 19,
  "critHitChance": 0.2,
  "normalHitChance": 0.5,
  "normalMissChance": 0.25,
  "critMissChance": 0.05
}
```

---

### `POST /api/v2/hitchance/calculate` *(requires authentication)*

API version 2 of the hit chance calculator. Functionally equivalent to v1 but requires a valid Bearer token in the `Authorization` header (see [Authentication](#authentication) below). The response drops the `toHit` and `defense` echo-back fields present in v1, returning only the calculated probability breakdown.

This endpoint demonstrates the per-controller versioning strategy: v2 introduces a breaking change to one resource's contract without affecting any other endpoint.

---

### `POST /api/v1/ParseDamage/calculate`

Parses a damage expression string and returns its components and average damage. Logs each request to the database.

### `GET /api/v1/ParseDamage/calculate`

Same calculation as the POST version but read-only — no logging, results are served from server-side memory cache on repeat inputs.

**Example:** `/api/v1/ParseDamage/calculate?expression=2d6%2B3+slashing`

**Supported formats:** `5`, `5 slashing`, `2d6`, `d8+3`, `2d6+3 fire`, `1d4-1 piercing`

**Example response:**

```json
{
  "originalExpression": "2d6+3 slashing",
  "damageDieCount": 2,
  "damageDieBase": 6,
  "damageModifier": 3,
  "damageType": "slashing",
  "averageDamage": 10.0,
  "parseStatus": "Parsed as slashing dice expression"
}
```

---

### `GET /api/v1/Logs`

Returns a paginated list of API request logs with optional filtering.

**Query parameters (all optional):**

| Parameter | Description | Default |
|---|---|---|
| `endpoint` | Partial match filter on endpoint name | — |
| `from` | Return logs on or after this date | — |
| `to` | Return logs on or before this date | — |
| `page` | Page number | 1 |
| `pageSize` | Results per page | 10 |

**Example:** `/api/v1/Logs?endpoint=Calculator&page=1&pageSize=10`

**Example response:**

```json
{
  "page": 1,
  "pageSize": 10,
  "totalRecords": 42,
  "totalPages": 5,
  "records": [...]
}
```

Log records are returned as `ApiRequestLogDto` objects — a decoupled response contract that is independent of the underlying EF entity, ensuring the API contract can evolve without being tied to database schema changes.

---

### `GET /api/v1/Logs/{id}`

Returns a single log entry by ID. Returns `404 Not Found` (as an RFC 7807 Problem Details response) if the ID does not exist.

---

### Damage Expression Shortcuts

When supplying `critHitDamage`, `normalMissDamage`, or `critMissDamage`, you can use keyword shortcuts relative to the normal hit damage:

| Keyword | Effect |
|---|---|
| *(blank)* | Use the default for that outcome (crit = ×2, miss = 0 or ½ for spells) |
| `dbl`, `double`, `2x`, `200%` | Double normal hit damage |
| `half`, `halved`, `1/2`, `50%` | Half normal hit damage |
| `triple`, `3x`, `300%` | Triple normal hit damage |
| `0`, `zero`, `none` | Zero damage |
| `2d6+3 fire` | Any valid damage expression |

---

## Authentication

The API uses **JWT (JSON Web Token)** bearer authentication on selected endpoints. JWT is a standard for self-contained, digitally signed tokens: the server issues a token containing claims (user identity, expiry time) and signs it with a secret key. On protected endpoints, the server validates the signature on every request without any database lookup — the token itself carries all the information needed.

To access a protected endpoint:

1. Call `POST /api/v1/Auth/token` with valid credentials to obtain a token
2. Include the token in subsequent requests: `Authorization: Bearer <token>`

**v1 endpoints are open** — no authentication required, preserving backward compatibility for existing consumers. **v2 endpoints require authentication**, reflecting a deliberate versioning strategy: breaking changes to the security contract are introduced in a new version rather than applied retroactively.

In production, the signing key would be stored in Azure Key Vault or as an environment variable — never in source control.

---

## Validation

Request validation is handled at the API layer via `IValidatableObject` and `[DataAnnotations]`, before any domain logic runs.

**Cross-field rules enforced:**
- `IsAgile = true` requires `HasMAP = true` (agile is meaningless without MAP)
- `IsDefaultAttack = true` on any attack requires a `DefaultAttack` template to be present
- Concrete attacks (`IsDefaultAttack = false`) must supply `BaseToHit` and `NormalHitDamage` directly
- `DefaultAttack.NormalHitDamage` is always required when `DefaultAttack` is provided

**Type-level separation:**
`DefaultAttackRequest` and `AttackRequest` are deliberately separate types with different validation contracts. `DefaultAttackRequest` has no `AttackNumber` or `IsDefaultAttack` (meaningless on the template) and validates `NormalHitDamage` unconditionally. `AttackRequest` validates `AttackNumber` (range 1–20) unconditionally and `BaseToHit`/`NormalHitDamage` only when `IsDefaultAttack = false`.

All validation errors return `400 Bad Request` with a structured RFC 7807 `ValidationProblemDetails` body.

---

## API Versioning

All endpoints are versioned via the URL path (`/api/v1/...`, `/api/v2/...`), implemented with the `Asp.Versioning` package. Each controller declares its version with `[ApiVersion("1.0")]` or `[ApiVersion("2.0")]`, and the version is resolved from the route itself (`api/v{version:apiVersion}/[controller]`).

Versioning is applied per-controller rather than globally — if a future change requires a breaking update to one resource's contract (say, `HitChance`), only that controller moves to `/api/v2/HitChance/...` while every other endpoint stays on v1, unaffected.

Swagger UI reflects the current version via a "Select a definition" dropdown, which lists each active version.

---

## Rate Limiting

The API applies a **fixed-window rate limit** of 30 requests per 10 seconds per client, using `Microsoft.AspNetCore.RateLimiting`. Requests that exceed the limit receive `429 Too Many Requests`. The `/health` endpoint is exempt from rate limiting so uptime monitors are never blocked.

---

## Error Handling

The API returns [RFC 7807 Problem Details](https://datatracker.ietf.org/doc/html/rfc7807) for all error responses:

- **Validation errors** return `400 Bad Request` with a structured `ValidationProblemDetails` body
- **Rate limit exceeded** returns `429 Too Many Requests`
- **Not-found responses** return a structured `ProblemDetails` body
- **Unhandled exceptions** return `500 Internal Server Error` with a structured `ProblemDetails` body via a global exception handler (`AddProblemDetails()` + `UseExceptionHandler()`), rather than a raw stack trace

**Custom domain exception types** — `BattleReady.Core` defines a hierarchy of exception types (`DomainException`, `NotFoundException`, `ValidationException`) that represent domain-level error conditions with no knowledge of HTTP. The API layer's global exception handler translates these to appropriate status codes in a single place:

| Exception type | HTTP status |
|---|---|
| `NotFoundException` | `404 Not Found` |
| `ValidationException` | `422 Unprocessable Entity` |
| `DomainException` (base) | `400 Bad Request` |
| Any other exception | `500 Internal Server Error` |

This gives any API consumer one consistent, machine-readable error shape to parse, regardless of failure mode.

---

## Health & Observability

**`GET /health`** reports application and SQL Server connectivity status (`Healthy` / `Degraded` / `Unhealthy`). The SQL Server check reports `Degraded` rather than `Unhealthy` on failure, since the Azure SQL free tier can have transient cold-start delays that aren't true outages.

**Structured logging** — all application logs are emitted as compact JSON via [Serilog](https://serilog.net/), written to both the console and a rolling daily file (`logs/log-{date}.json`, gitignored). Every log line for a given HTTP request shares the same `CorrelationId` (propagated via `RequestLoggingMiddleware` using Serilog's `LogContext`), making it straightforward to trace all activity for a single request across the log stream.

**Request logging** — POST actions on the Calculator, HitChance, and ParseDamage controllers log each request and response to the database via `RequestLoggingFilter` (an `IAsyncActionFilter`). The endpoint string is derived from the live ASP.NET Core route at runtime — never hardcoded. GET endpoints are intentionally excluded from logging.

---

## Solution Structure

```
BattleReady.slnx
├── BattleReady.Core/              # Shared class library — models and services
│   ├── Exceptions/                # DomainException (abstract), NotFoundException, ValidationException
│   └── Features/Calculator/
│       ├── Models/                # Input/response models
│       │   └── Shared/            # DegreeOfSuccess enum, DegreeOfSuccessCalculator (pure static logic)
│       └── Services/              # CalculationService, HitChanceService, SpellSaveService,
│                                  #   ParseDamageService, DegreeOfSuccessService
│                                  #   + corresponding interfaces
├── BattleReady.Api/               # ASP.NET Core Web API
│   ├── Controllers/               # CalculatorController, HitChanceController, HitChanceV2Controller,
│   │                              #   ParseDamageController, LogsController, AuthController
│   ├── Filters/                   # RequestLoggingFilter (IAsyncActionFilter — DB logging on POST actions)
│   ├── Mapping/                   # Extension methods: *Request.ToInput(), ApiRequestLog.ToDto()
│   ├── Middleware/                # RequestLoggingMiddleware (CorrelationId → Serilog LogContext)
│   ├── Models/Requests/           # AttackRequest, DefaultAttackRequest, CalculationRequest,
│   │                              #   SpellSaveRequest, HitChanceRequest, LoginRequest, etc.
│   ├── Models/Responses/          # LogsResponse, ApiRequestLogDto (decoupled from EF entity)
│   └── Program.cs                 # DI, Serilog bootstrap, JWT auth, versioning, health checks,
│                                  #   rate limiting, global exception handler, Problem Details
├── BattleReady.Data/              # EF Core data access layer
│   ├── Entities/                  # ApiRequestLog entity (bounded string columns on Endpoint, RequestBody)
│   ├── Migrations/                # EF Core migrations (auto-applied on startup via db.Database.Migrate())
│   └── AppDbContext.cs
├── BattleReady.Console/           # Original console prototype
└── BattleReady.Tests/             # xUnit test project — 89 tests total
    ├── HitChance/                 # HitChanceServiceTests, DegreeOfSuccessCalculatorTests ([Theory]-based)
    ├── ParseDamage/               # ParseDamageServiceTests
    ├── SpellSave/                 # SpellSaveServiceTests
    ├── Calculator/                # CalculationServiceTests, CalculationServiceSpellSaveTests
    │                              #   — Moq-based unit tests of orchestration logic
    └── Integration/               # Full HTTP-stack tests via WebApplicationFactory + in-memory database
                                   #   Covers: Calculator, HitChance, HitChanceV2, ParseDamage, Logs,
                                   #   Auth (JWT token issuance + protected endpoint), health check,
                                   #   global exception handler (500, 404, 422, 400 domain exceptions)
```

---

## Key Design Decisions

**Layer separation** — `BattleReady.Core` has zero knowledge of the API or database layers. Dependency arrows always point inward: Api → Core, Data → Core, never the reverse.

**Dependency Inversion** — Controllers and `CalculationService` depend on interfaces (`IHitChanceService`, `IParseDamageService`, `ICalculationService`, `ISpellSaveService`), not concrete classes. This enables `CalculationServiceTests` to use Moq to mock all dependencies and verify orchestration logic in isolation, without depending on the real hit-chance or damage-parsing math.

**Custom domain exception types** — `BattleReady.Core` defines an exception vocabulary (`DomainException`, `NotFoundException`, `ValidationException`) that expresses domain-level error conditions without any knowledge of HTTP. Core services throw these when something goes wrong; the API layer's global exception handler intercepts them in one place and translates them to appropriate HTTP status codes. This keeps the translation logic centralized rather than scattered across every controller action.

**JWT authentication** — `POST /api/v1/Auth/token` issues signed JWTs containing user identity claims. The server validates the token signature on each request using a shared secret — no session storage or database lookup required. v1 endpoints remain open for backward compatibility; v2 endpoints require authentication, making the security boundary explicit in the API version rather than buried in middleware configuration.

**Request/Input separation** — Request models (`*Request`) live in the API layer with validation attributes and `IValidatableObject`. Core input models (`*Input`) are plain C# with no framework dependencies. Controllers map between them via extension methods in `Mapping/`.

**DefaultAttackRequest vs AttackRequest** — `DefaultAttackRequest` is a deliberately separate type from `AttackRequest`. A default attack template has no `AttackNumber` (meaningless on a template) and no `IsDefaultAttack` flag. Keeping them as one type forced context-dependent validation that was fragile and hard to reason about; two types with distinct contracts is the honest model.

**DTO decoupling** — The `GET /api/v1/Logs` response returns `ApiRequestLogDto` rather than the raw `ApiRequestLog` EF entity. This prevents the API contract from being accidentally coupled to database schema changes, and makes the response shape explicitly owned by the API layer.

**Cooperative cancellation** — All async controller actions accept a `CancellationToken`, threaded through to EF Core's async calls for the primary query work. The one deliberate exception is the post-success logging write in `RequestLoggingFilter` — cancellation is intentionally omitted there, because a client disconnect after a successful response should not prevent that request from being recorded in the audit log.

**GET vs POST** — The Calculator endpoint uses POST because it logs to the database (side effect). HitChance and ParseDamage offer both: POST with logging for stateful clients, GET without logging for pure read-only calculations. GET responses are server-side cached via `IMemoryCache`; cache keys include all inputs.

**RequestLoggingFilter placement** — `[ServiceFilter(typeof(RequestLoggingFilter))]` is applied at the method level on POST actions only, not at the class level. On controllers that expose both GET and POST actions, class-level placement would incorrectly log GET requests.

**Rate limiting** — Fixed-window limiting is applied globally via `MapControllers().RequireRateLimiting("fixed")`. The `/health` endpoint opts out via `.DisableRateLimiting()` so monitoring probes are never throttled.

**Column size constraints** — `ApiRequestLog.Endpoint` is capped at `nvarchar(500)` and `RequestBody` at `nvarchar(4000)` via `[MaxLength]`. `ResponseBody` is intentionally left as `nvarchar(max)` — response bodies for large attack sequences can exceed 4,000 characters (SQL Server's maximum bounded `nvarchar` length), so truncating would produce incomplete log records.

**Auto-migrations** — `db.Database.Migrate()` runs on startup. Schema changes deploy automatically with the code; no manual SQL steps are required.

**Secure secrets** — The production connection string and JWT signing key are stored as Azure App Service environment variables, never in source code or `appsettings.json`.

**CI/CD pipeline** — GitHub Actions builds, runs all 89 tests, and deploys on every push to `master`. A failing test blocks deployment.

---

## Running Locally

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download), SQL Server or SQL Server Express

```bash
git clone https://github.com/mark-raymond-dev/BattleReady.git
cd BattleReady
```

Add your local SQL Server connection string to `BattleReady.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_INSTANCE;Database=BattleReadyDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Then run:

```bash
dotnet run --project BattleReady.Api
```

The database and tables are created automatically on first run. Swagger UI will be available at `http://localhost:{port}/swagger` and the health check at `http://localhost:{port}/health`.

Structured logs are written to the console and to `logs/log-{date}.json` (relative to the `BattleReady.Api` working directory) on every run.

**Firing requests locally** — the repo includes `BattleReady.Api/BattleReady.Api.http` with pre-built requests for every endpoint. Install the [REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) for VS Code, start the API, and click "Send Request" above any block.

To run all 89 tests:

```bash
dotnet test
```

---

## Tech Stack

- .NET 10 / C#
- ASP.NET Core Web API
- Entity Framework Core 10 with SQL Server
- Swashbuckle (Swagger / OpenAPI)
- Asp.Versioning (URL-based API versioning)
- Microsoft.AspNetCore.Authentication.JwtBearer (JWT bearer authentication)
- Microsoft.AspNetCore.RateLimiting (fixed-window rate limiting)
- Microsoft.Extensions.Caching.Memory (server-side IMemoryCache on GET endpoints)
- Serilog (structured JSON logging — console + rolling file sinks)
- AspNetCore.HealthChecks.SqlServer (health check endpoint with DB connectivity probe)
- xUnit (unit tests + integration tests via `WebApplicationFactory`)
- Moq (mocked dependencies for isolated unit testing of orchestration logic)
- Azure App Service (Free F1 tier) + Azure SQL Database (free offer)
- GitHub Actions (CI/CD — builds, tests, and deploys on push to `master`)

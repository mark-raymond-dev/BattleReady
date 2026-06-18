# BattleReady

A Pathfinder 2e combat calculator that computes expected damage per round across multiple attacks.
Built as a portfolio project to demonstrate modern .NET development practices including clean architecture, dependency inversion, Entity Framework Core, REST API design, structured logging, API versioning, and both unit and integration testing.

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

All endpoints are versioned under `/api/v1/`. See [API Versioning](#api-versioning) below.

### `POST /api/v1/Calculator/calculate`

The primary endpoint. Submits a full attack sequence and receives per-attack breakdowns plus a grand total. Logs each request to the database.

**Example request:**

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

**Example response (abbreviated):**

```json
{
  "attackResponses": [
    {
      "attackNumber": 1,
      "effectiveToHit": 12,
      "effectiveDefense": 19,
      "critHitChance": 0.2,
      "normalHitChance": 0.65,
      "normalMissChance": 0.1,
      "critMissChance": 0.05,
      "avgDmgCritHit": 19.0,
      "avgDmgNormalHit": 9.5,
      "avgDmgNormalMiss": 0.0,
      "avgDmgCritMiss": 0.0,
      "totalExpectedDamage": 9.925
    }
  ],
  "totalExpectedDamageAllAttacks": 19.25,
  "calculatedAt": "2026-06-09T00:00:00Z"
}
```

---

### `POST /api/v1/HitChance/calculate`

Calculates hit chance breakdown for a single attack roll. Logs each request to the database.

### `GET /api/v1/HitChance/calculate`

Same calculation as the POST version but read-only — no logging. Parameters are passed as query string values, making results bookmarkable and cacheable.

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

### `POST /api/v1/ParseDamage/calculate`

Parses a damage expression string and returns its components and average damage. Logs each request to the database.

### `GET /api/v1/ParseDamage/calculate`

Same calculation as the POST version but read-only — no logging, results are cacheable.

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

## API Versioning

All endpoints are versioned via the URL path (`/api/v1/...`), implemented with the `Asp.Versioning` package. Each controller declares its version with `[ApiVersion("1.0")]`, and the version is resolved from the route itself (`api/v{version:apiVersion}/[controller]`).

Versioning is applied per-controller rather than globally — if a future change requires a breaking update to one resource's contract (say, `HitChance`), only that controller moves to `/api/v2/HitChance/...` while every other endpoint stays on v1, unaffected. Existing consumers of an unversioned-by-them endpoint are never forced into a breaking change just because a different part of the API evolved.

Swagger UI reflects the current version via a "Select a definition" dropdown, which will list each active version once more than one exists.

---

## Error Handling

The API returns [RFC 7807 Problem Details](https://datatracker.ietf.org/doc/html/rfc7807) for all error responses:

- **Validation errors** (e.g. missing required fields) return `400 Bad Request` with a structured `ValidationProblemDetails` body — handled automatically by `[ApiController]`.
- **Unhandled exceptions** return `500 Internal Server Error` with a structured `ProblemDetails` body, via a global exception handler (`AddProblemDetails()` + `UseExceptionHandler()`), instead of a raw stack trace or an unstructured error.
- **Not-found responses** (e.g. `GET /api/v1/Logs/{id}` for a missing record) also return a structured `ProblemDetails` body.

This gives any API consumer one consistent, machine-readable error shape to parse, rather than a different ad-hoc format per failure mode.

---

## Health & Observability

- **`GET /health`** — reports application and SQL Server connectivity status (`Healthy` / `Degraded` / `Unhealthy`), suitable for uptime monitoring or as a probe target for App Service / container orchestrators. The SQL Server check is configured to report `Degraded` rather than `Unhealthy` on failure, since the underlying Azure SQL free tier can have transient cold-start delays that aren't true outages.
- **Structured logging** — all application logs are emitted as compact JSON via [Serilog](https://serilog.net/), written to both the console and a rolling daily file (`logs/log-{date}.json`, gitignored). JSON-structured logs are ready for ingestion by log aggregation tooling (e.g. Azure Monitor, Seq) without regex parsing, and every field (timestamp, level, message, request context) is independently queryable rather than embedded in a formatted sentence.

---

## Solution Structure

```
BattleReady.slnx
├── BattleReady.Core/          # Shared class library — models and services
│   └── Features/Calculator/
│       ├── Models/            # Input/response models
│       │   └── Shared/        # DegreeOfSuccess enum, DegreeOfSuccessCalculator (pure static logic)
│       └── Services/          # CalculationService, HitChanceService, ParseDamageService
│                               #   + ICalculationService, IHitChanceService, IParseDamageService
├── BattleReady.Api/           # ASP.NET Core Web API
│   ├── Controllers/           # CalculatorController, HitChanceController, ParseDamageController, LogsController
│   ├── Mapping/                # Extension methods: *Request.ToInput()
│   ├── Models/Requests/       # API request models with validation attributes
│   ├── Models/Responses/      # LogsResponse with pagination metadata
│   └── Program.cs              # DI registrations, Serilog bootstrap, versioning, health checks, Problem Details
├── BattleReady.Data/           # EF Core data access layer
│   ├── Entities/                # ApiRequestLog entity
│   ├── Migrations/              # EF Core migrations (auto-applied on startup)
│   └── AppDbContext.cs
├── BattleReady.Console/        # Original console prototype
└── BattleReady.Tests/          # xUnit test project — 45 tests total
    ├── HitChance/               # HitChanceServiceTests, DegreeOfSuccessCalculatorTests ([Theory]-based)
    ├── ParseDamage/              # ParseDamageServiceTests
    ├── Calculator/                # CalculationServiceTests — Moq-based unit tests of orchestration logic
    └── Integration/                # Full HTTP-stack tests via WebApplicationFactory + in-memory database,
                                      #   including health check coverage
```

---

## Key Design Decisions

**Layer separation** — `BattleReady.Core` has zero knowledge of the API or database layers. Dependency arrows always point inward: Api → Core, Data → Core, never the reverse.

**Dependency Inversion** — Controllers and `CalculationService` depend on interfaces (`IHitChanceService`, `IParseDamageService`, `ICalculationService`), not concrete classes, registered via `AddScoped<IXxxService, XxxService>()`. This is what enables true unit testing of `CalculationService`'s orchestration logic in isolation — `CalculationServiceTests` uses Moq to mock both dependencies and verify the orchestration behaves correctly without depending on the real hit-chance or damage-parsing math.

**Request/Input separation** — Request models (`*Request`) live in the API layer with validation attributes. Core input models (`*Input`) are plain C# with no framework dependencies. Controllers map between them via extension methods in `Mapping/`.

**Cooperative cancellation** — All async controller actions accept a `CancellationToken`, threaded through to EF Core's async calls (`SaveChangesAsync`, `ToListAsync`, `FindAsync`, etc.). If a client disconnects mid-request, the server abandons the in-flight database work rather than completing it unnecessarily.

**GET vs POST** — The Calculator endpoint uses POST because it logs to the database on every call (side effect). The HitChance and ParseDamage endpoints offer both: POST with logging for stateful clients, GET without logging for pure read-only calculations. GET responses are marked cacheable with `[ResponseCache]`.

**Per-controller API versioning** — Each controller is versioned independently via `[ApiVersion("1.0")]`, rather than versioning the entire API as one unit. A breaking change to one resource doesn't force a version bump on resources that haven't changed.

**Structured error responses** — All errors (validation, not-found, unhandled exceptions) return RFC 7807 Problem Details via `AddProblemDetails()` + `UseExceptionHandler()`, giving consumers one consistent error shape instead of inconsistent per-endpoint formats.

**Auto-migrations** — `db.Database.Migrate()` runs on startup, so schema changes deploy automatically with the code. No manual SQL steps required.

**Secure secrets** — The production connection string is stored as an Azure App Service environment variable, never in source code or `appsettings.json`.

**CI/CD pipeline** — GitHub Actions builds, runs all 45 tests, and deploys on every push to `master`. A failing test blocks deployment.

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

The database and tables are created automatically on first run. Swagger UI will be available at `https://localhost:{port}/swagger`, and the health check at `https://localhost:{port}/health`.

Structured logs are written to the console and to `logs/log-{date}.json` (relative to the `BattleReady.Api` working directory) on every run.

To run all 45 tests:

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
- Serilog (structured JSON logging — console + rolling file sinks)
- AspNetCore.HealthChecks.SqlServer (health check endpoint with DB connectivity probe)
- xUnit (unit tests + integration tests via `WebApplicationFactory`)
- Moq (mocked dependencies for isolated unit testing of orchestration logic)
- Azure App Service (Free F1 tier) + Azure SQL Database (free offer)
- GitHub Actions (CI/CD — builds, tests, and deploys on push to `master`)

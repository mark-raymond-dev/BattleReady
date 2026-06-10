# BattleReady

A Pathfinder 2e combat calculator that computes expected damage per round across multiple attacks.
Built as a portfolio project to demonstrate modern .NET development practices including clean architecture, Entity Framework Core, REST API design, CI/CD, and both unit and integration testing.

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

> **Note:** The API is hosted on Azure's Free F1 tier and may take 30–60 seconds to wake up after a period of inactivity.

---

## API Endpoints

### `POST /api/Calculator/calculate`

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

### `POST /api/HitChance/calculate`

Calculates hit chance breakdown for a single attack roll. Logs each request to the database.

### `GET /api/HitChance/calculate`

Same calculation as the POST version but read-only — no logging. Parameters are passed as query string values, making results bookmarkable and cacheable.

**Example:** `/api/HitChance/calculate?toHit=12&defense=19&natural20Upgrades=true&natural1Downgrades=true`

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

### `POST /api/ParseDamage/calculate`

Parses a damage expression string and returns its components and average damage. Logs each request to the database.

### `GET /api/ParseDamage/calculate`

Same calculation as the POST version but read-only — no logging, results are cacheable.

**Example:** `/api/ParseDamage/calculate?expression=2d6%2B3+slashing`

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

### `GET /api/Logs`

Returns a paginated list of API request logs with optional filtering.

**Query parameters (all optional):**

| Parameter | Description | Default |
|---|---|---|
| `endpoint` | Partial match filter on endpoint name | — |
| `from` | Return logs on or after this date | — |
| `to` | Return logs on or before this date | — |
| `page` | Page number | 1 |
| `pageSize` | Results per page | 10 |

**Example:** `/api/Logs?endpoint=Calculator&page=1&pageSize=10`

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

### `GET /api/Logs/{id}`

Returns a single log entry by ID. Returns `404 Not Found` if the ID does not exist.

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

## Solution Structure

```
BattleReady.slnx
├── BattleReady.Core/          # Shared class library — models and services
│   └── Features/Calculator/
│       ├── Models/            # Input/response models, DegreeOfSuccess enum
│       └── Services/          # CalculationService, HitChanceService, ParseDamageService
├── BattleReady.Api/           # ASP.NET Core Web API
│   ├── Controllers/           # CalculatorController, HitChanceController, ParseDamageController, LogsController
│   ├── Mapping/               # Extension methods: *Request.ToInput()
│   ├── Models/Requests/       # API request models with validation attributes
│   ├── Models/Responses/      # LogsResponse with pagination metadata
│   └── Program.cs
├── BattleReady.Data/          # EF Core data access layer
│   ├── Entities/              # ApiRequestLog entity
│   ├── Migrations/            # EF Core migrations (auto-applied on startup)
│   └── AppDbContext.cs
├── BattleReady.Console/       # Original console prototype
└── BattleReady.Tests/         # xUnit test project
    ├── HitChance/             # HitChanceServiceTests (13 unit tests)
    ├── ParseDamage/           # ParseDamageServiceTests (13 unit tests)
    └── Integration/           # Integration tests (9 tests) — full HTTP stack with in-memory database
```

---

## Key Design Decisions

**Layer separation** — `BattleReady.Core` has zero knowledge of the API or database layers. Dependency arrows always point inward: Api → Core, Data → Core, never the reverse.

**Request/Input separation** — Request models (`*Request`) live in the API layer with validation attributes. Core input models (`*Input`) are plain C# with no framework dependencies. Controllers map between them via extension methods in `Mapping/`.

**GET vs POST** — The Calculator endpoint uses POST because it logs to the database on every call (side effect). The HitChance and ParseDamage endpoints offer both: POST with logging for stateful clients, GET without logging for pure read-only calculations. GET responses are marked cacheable with `[ResponseCache]`.

**Auto-migrations** — `db.Database.Migrate()` runs on startup, so schema changes deploy automatically with the code. No manual SQL steps required.

**Secure secrets** — The production connection string is stored as an Azure App Service environment variable, never in source code or `appsettings.json`.

**CI/CD pipeline** — GitHub Actions builds, runs all 35 tests, and deploys on every push to `master`. A failing test blocks deployment.

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

The database and tables are created automatically on first run. Swagger UI will be available at `https://localhost:{port}/swagger`.

To run all tests:

```bash
dotnet test
```

---

## Tech Stack

- .NET 10 / C#
- ASP.NET Core Web API
- Entity Framework Core 10 with SQL Server
- Swashbuckle (Swagger / OpenAPI)
- xUnit (unit tests + integration tests via `WebApplicationFactory`)
- Azure App Service (Free F1 tier) + Azure SQL Database (free offer)
- GitHub Actions (CI/CD — builds, tests, and deploys on push to `master`)

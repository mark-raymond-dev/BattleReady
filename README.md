# BattleReady

A Pathfinder 2e combat calculator that computes expected damage per round across multiple attacks.
Built as a portfolio project to demonstrate ASP.NET Core REST API design, clean architecture, dependency injection, input validation, and unit testing.

---

## What It Does

BattleReady models a full attack sequence and calculates **expected damage per round** by accounting for:

- **Multiple Attack Penalty (MAP)** — standard (−5/−10) or agile (−4/−8)
- **Degrees of success** — Critical Hit, Hit, Miss, and Critical Miss, each with configurable damage
- **Natural 20 upgrades** and **Natural 1 downgrades** (toggleable)
- **Dice-based damage expressions** — e.g. `2d6+3 slashing`, `d8`, `5 fire`
- **Saving throw spells** — auto-applies half damage on a Miss and zero on a Critical Miss by default
- **Default attack templates** — define a base attack once and reuse it across an entire sequence

---

## Live Demo

Swagger UI: [https://battleready-api-b4h4brhga5dea5ay.westus3-01.azurewebsites.net/swagger](https://battleready-api-b4h4brhga5dea5ay.westus3-01.azurewebsites.net/swagger)

---

## API Endpoints

### `POST /api/Calculator/calculate`

The primary endpoint. Submits a full attack sequence and receives per-attack breakdowns plus a grand total.

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
    },
    ...
  ],
  "totalExpectedDamageAllAttacks": 19.25,
  "calculatedAt": "2025-01-01T00:00:00Z"
}
```

---

### `POST /api/HitChance/calculate`

Calculates hit chance breakdown for a single attack roll against a single defense value.

**Example request:**

```json
{
  "toHit": 12,
  "defense": 19,
  "natural20Upgrades": true,
  "natural1Downgrades": true
}
```

**Example response:**

```json
{
  "toHit": 12,
  "defense": 19,
  "critHitChance": 0.2,
  "normalHitChance": 0.65,
  "normalMissChance": 0.1,
  "critMissChance": 0.05
}
```

---

### `POST /api/ParseDamage/calculate`

Parses a damage expression string and returns its components and average damage.

**Supported formats:** `5`, `5 slashing`, `2d6`, `d8+3`, `2d6+3 fire`, `1d4-1 piercing`

**Example request:**

```json
{
  "expression": "2d6+3 slashing"
}
```

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
│   ├── Controllers/           # CalculatorController, HitChanceController, ParseDamageController
│   ├── Mapping/               # Extension methods: *Request.ToInput()
│   ├── Models/Requests/       # API request models with validation attributes
│   └── Program.cs
├── BattleReady.Console/       # Original console prototype
└── BattleReady.Tests/         # xUnit test project
    ├── HitChance/             # HitChanceServiceTests (13 tests)
    └── ParseDamage/           # ParseDamageServiceTests (13 tests)
```

**Key design decisions:**

- `BattleReady.Core` has zero knowledge of the API layer — all ASP.NET concerns live in `BattleReady.Api`
- Request models (`*Request`) live in the API layer with validation attributes; Core input models (`*Input`) are plain C# with no framework dependencies
- Controllers map between them via extension methods in `Mapping/`
- Services are registered as scoped and injected via constructor injection throughout
- `[ApiController]` handles `ModelState` validation automatically — no manual checks in controllers

---

## Running Locally

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
git clone https://github.com/mark-raymond-dev/BattleReady.git
cd BattleReady
dotnet run --project BattleReady.Api
```

Swagger UI will be available at `https://localhost:{port}/swagger`.

To run the console prototype instead:

```bash
dotnet run --project BattleReady.Console
```

To run the unit tests:

```bash
dotnet test
```

---

## Tech Stack

- .NET 10 / C#
- ASP.NET Core Web API
- Swashbuckle (Swagger / OpenAPI)
- xUnit
- Azure App Service (Free F1 tier)
- GitHub Actions (CI/CD — deploys on push to `master`)

# System Process Cotation

> Event-driven stock-price monitor built in **.NET 9 / C#**. It continuously tracks an asset's price and sends a **buy/sell email alert** when the price crosses a target threshold.

![Demo](SystemProcessCotation/assets/demo.gif)

---

## What this project demonstrates

Beyond the feature itself, the codebase is a compact showcase of production-style backend patterns:

- **Event-driven pipeline** — three decoupled background workers communicating over a message bus (producer → analyzer → notifier).
- **Pub/Sub messaging** — an SNS (topics) + SQS (queues) event bus, runnable fully offline via **LocalStack** or against real AWS.
- **Clean architecture & DI** — every component sits behind an interface and is wired through the .NET Generic Host container; business rules are isolated from I/O.
- **Stateful deduplication** — **Redis** stores the last alert price and a cooldown window, so users aren't spammed with repeated alerts.
- **Graceful degradation** — with no SMTP configured, alerts are logged instead of emailed, so the whole system is demoable without credentials.
- **Containerized** — a single `docker compose up` spins up the app, Redis, and the LocalStack AWS emulator.

---

## Architecture

```
                 ┌──────────────────┐   publishes   ┌──────────────┐
  price source   │  CotationWorker  │ ────────────▶ │   cotations  │
 (web scrape) ──▶│    (producer)    │               │   (bus topic)│
                 └──────────────────┘               └──────┬───────┘
                                                           │ subscribes
                                                           ▼
                 ┌──────────────────┐   Redis dedup  ┌──────────────┐
    email    ◀───│ NotificationWkr  │ ◀───────────── │ TradingWorker│
   (MailKit)     │   (consumer)     │   publishes    │(analyzer)    │
                 └──────────────────┘  ┌──────────┐  └──────┬───────┘
                          ▲            │  alerts   │◀────────┘
                          └────────────│ (bus topic)│
                                       └──────────┘
```

1. **CotationWorker** (producer) — on a fixed interval, fetches the asset's current price and publishes it to the `cotations` channel.
2. **TradingWorker** (analyzer) — subscribes to `cotations`, applies the buy/sell rule, filters out repeats via a Redis cooldown, and publishes qualifying `alerts`.
3. **NotificationWorker** (consumer) — subscribes to `alerts` and sends the email (or logs it if SMTP is off).

---

## Tech stack

| Area | Technology |
|------|-----------|
| Runtime | .NET 9, C# |
| Hosting / DI | `Microsoft.Extensions.Hosting` (Generic Host, `BackgroundService`) |
| Messaging | AWS **SNS + SQS** (`AWSSDK`), LocalStack for local dev |
| State | **Redis** (`StackExchange.Redis`) |
| Email | **MailKit** (SMTP) |
| Price scraping | `HttpClient` + **HtmlAgilityPack** |
| Config | `appsettings.json`, command-line args, `.env` (`DotNetEnv`) |
| Infra | Docker, Docker Compose |

> **Note on the price source:** prices are scraped from a public Brazilian equities page (Fundamentus). Scrapers are inherently fragile — treat the fetch layer as a swappable adapter behind `ICotationService`, and plug in a proper market-data API for production use.

---

## Getting started

### Option 1 — Docker Compose (recommended)

Brings up the worker, Redis, and LocalStack together:

```bash
docker compose up --build
```

The default thresholds in `docker-compose.yml` are set to force a demo **SELL** alert. Adjust the asset and prices there for your own scenario.

### Option 2 — Run locally with .NET

You'll need the **.NET 9 SDK**, plus Redis and LocalStack (or real AWS) reachable at the addresses in `appsettings.json`.

```bash
cd SystemProcessCotation

dotnet run -- --help

# Usage: dotnet run <ASSET> <sellPrice> <buyPrice> [checkIntervalMs] [alertCooldownSeconds]
dotnet run PETR4 22.67 22.59
```

Help can be opened with `help`, `-h`, `--help`, `-?`, `/h`, or `/?`.

Command-line arguments take priority; if omitted, values are read from the `Trading` section of `appsettings.json` or from deployment-friendly aliases. The optional `checkIntervalMs` and `alertCooldownSeconds` arguments let you tune demo cadence without editing configuration:

```bash
dotnet run PETR4 22.67 22.59 1000 15
```

Stock symbols are normalized from common B3 copies such as `BVMF:PETR4`, `PETR4.SA`, `PETR4.BVMF`, or `PETR4.BOVESPA`.

Price thresholds accept dot or comma decimals and optional currency markers in command-line and configuration values, including copied text such as `R$ 35,50`, `R $ 35,50`, `BRL 35,50`, or `1'234.56`. Mixed quote snippets also work, such as `31,42 +0,50%` or `12/09/2026 31,42`.

### Build a standalone executable

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

# then run it:
bin\Release\net9.0\win-x64\publish\SystemProcessCotation.exe PETR4 35.00 30.00
```

### Run via the PowerShell helper script

```powershell
Set-ExecutionPolicy Unrestricted   # once, if scripts are blocked
cd .\SystemProcessCotation\
.\script.ps1 PETR4 32.54 32.51
```

---

## Configuration

### Trading & infrastructure — `appsettings.json`

```jsonc
{
  "Aws":   { "Region": "us-east-1", "ServiceUrl": "http://localhost:4566" },
  "Redis": { "ConnectionString": "localhost:6379" },
  "Trading": {
    "StockSymbol": "PETR4",
    "PriceToSell": 999.0,     // alert when price >= this
    "PriceToBuy":  0.01,      // alert when price <= this
    "CheckIntervalMs": 5000,  // polling interval
    "AlertCooldownSeconds": 60 // suppress repeated alerts
  }
}
```

- Leave `Aws:ServiceUrl` empty to target **real AWS** (uses the default credential chain); set it to the LocalStack URL for offline runs.
- Trading aliases are accepted when nested config keys are inconvenient: `TRADING_STOCK_SYMBOL`, `STOCK_SYMBOL`, or `ASSET_SYMBOL`; `TRADING_PRICE_TO_SELL`, `PRICE_TO_SELL`, or `SELL_PRICE`; `TRADING_PRICE_TO_BUY`, `PRICE_TO_BUY`, or `BUY_PRICE`; `TRADING_CHECK_INTERVAL_MS`, `CHECK_INTERVAL_MS`, or `POLL_INTERVAL_MS`; and `TRADING_ALERT_COOLDOWN_SECONDS`, `ALERT_COOLDOWN_SECONDS`, or `ALERT_COOLDOWN`.
- Infrastructure aliases are also accepted: `AWS_ENDPOINT_URL`, `AWS_SERVICE_URL`, or `LOCALSTACK_URL` for the AWS endpoint; `AWS_REGION` or `AWS_DEFAULT_REGION` for the region; and `REDIS_CONNECTION_STRING` or `REDIS_URL` for Redis.

### Email (SMTP) — optional

SMTP is **optional**. Without it, alerts are written to the log instead of emailed. To enable email, copy `.env.example` to `.env` and fill in:

```bash
cp .env.example .env
```

```env
HOST=smtp.gmail.com
PORT=587
USERNAME=your_user
PASSWORD=your_app_password
FROM=from@example.com
TO=to@example.com
ENABLE_SSL=true
```

The short SMTP keys above are still the default examples. The app also accepts common deployment aliases: `SMTP_HOST`, `SMTP_PORT`, `SMTP_USERNAME` or `SMTP_USER`, `SMTP_PASSWORD`, `SMTP_FROM`, `SMTP_TO`, and `SMTP_ENABLE_SSL` or `SMTP_SSL`. Hosting providers that reserve `SMTP_*` names can use the `MAIL_*` variants instead, including `MAIL_HOST`, `MAIL_PORT`, `MAIL_USERNAME` or `MAIL_USER`, `MAIL_PASSWORD`, `MAIL_FROM`, `MAIL_TO`, and `MAIL_ENABLE_SSL` or `MAIL_SSL`.

Use a plain SMTP hostname for `HOST`/`SMTP_HOST`, such as `smtp.gmail.com`; URL values like `https://smtp.gmail.com` are treated as not configured.

---

## Project structure

```
SystemProcessCotation/
├── Program.cs                # Host + DI wiring, config resolution
├── Workers/                  # BackgroundService pipeline
│   ├── CotationWorker.cs     #   producer: fetch & publish prices
│   ├── TradingWorker.cs      #   analyzer: buy/sell rule + Redis dedup
│   └── NotificationWorker.cs #   consumer: send/log email
├── Bus/                      # SNS/SQS event bus + provisioning
├── Services/                 # Cotation, Trading, Email, Redis state, Config
├── Interfaces/               # Contracts for every service (clean architecture)
├── Models/                   # CotationResult, TradingAlert
├── Utils/                    # Command-line parsing
└── appsettings.json
```

---

## License

Released for portfolio and educational purposes. Feel free to explore and adapt.

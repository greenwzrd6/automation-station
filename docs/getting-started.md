# Getting Started

## Prerequisites

- .NET SDK (the solution targets `net10.0`)
- Docker (for SQL Server and migrations)
- A reachable RabbitMQ instance (e.g. the one used by the Kanban backend)

## 1. Start the database

From the solution root (`automation-station/`):

```bash
docker compose up -d
```

This starts SQL Server on port **1435**, creates the `AutomationStationDb` database, and runs the Flyway migrations from `AutomationStation.Infrastructure/Migrations`.

## 2. Configure the worker

Connection settings live in `AutomationStation.Worker/appsettings.json` and `appsettings.Development.json`:

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `RabbitMQ:*` | Host, credentials, exchange (`placement.events`) and queue (`automation.placement.events`) |
| `KanbanApi:BaseUrl` | Base URL of the Kanban API |
| `TojApi:BaseUrl`, `TojApi:ApiKey` | Base URL and API key of the TOJ API |

Adjust these to your environment before running.

## 3. Run the worker

```bash
dotnet run --project AutomationStation.Worker
```

The worker connects to RabbitMQ, declares the exchange/queue/binding, and starts handling events. Each message is deserialized, evaluated against your automations, and any matching actions are dispatched to the Kanban/TOJ APIs.

## Next steps

- Define automations in the database (`Automations` table) — see the [Core domain model](xref:AutomationStation.Core.Automations.Automation) for the structure
- Browse the [API reference](xref:AutomationStation.Core.Automations.Automation) for details on every type

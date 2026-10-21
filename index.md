---
_layout: landing
---

# AutomationStation

Event-driven automation engine for the Kanban platform. It consumes integration events from RabbitMQ, evaluates user-defined automations against them, and executes actions in external systems.

## What it does

- **Listens** for integration events (e.g. placement events) on RabbitMQ
- **Evaluates** automations: a trigger plus optional conditions must match for an automation to fire
- **Acts** by calling the Kanban API (create placements) and the TOJ API (create/update tasks), with rate limiting and execution history

## Solution layout

| Project | Role |
| --- | --- |
| `AutomationStation.Core` | Domain model: `Automation`, `When`/`Trigger`/`Condition`, `Then` |
| `AutomationStation.Application` | Engine services, policies, and abstractions |
| `AutomationStation.Infrastructure` | Persistence, action executors, Kanban/TOJ integrations |
| `AutomationStation.Worker` | Host entry point and the RabbitMQ consumer |

Head over to [Introduction](docs/introduction.md) for the architecture, or [Getting Started](docs/getting-started.md) to run it locally.

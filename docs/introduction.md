# Introduction

AutomationStation is a background worker that automates work across systems in response to events. An **automation** is a rule with two parts:

- **When** — a [trigger](xref:AutomationStation.Core.Automations.Whens.Trigger) (event type + source system) and zero or more [conditions](xref:AutomationStation.Core.Automations.Whens.Condition) that are evaluated against the event
- **Then** — one or more actions (e.g. create a placement, create or update a TOJ task)

When an event arrives on RabbitMQ, the engine finds all enabled automations whose trigger matches the event, evaluates their conditions against the event's fields, and dispatches the resulting actions through registered action handlers.

## Architecture

```
RabbitMQ ──> Worker ──> AutomationProcessor ──> ActionDispatcher ──> Kanban API / TOJ API
                             │                        │
                             ▼                        ▼
                        SQL Server            Rate limiter, history
```

- **Core** is pure domain: the [Automation](xref:AutomationStation.Core.Automations.Automation) aggregate and its `When`/`Then` parts, with no dependencies on infrastructure.
- **Application** contains the engine: [AutomationProcessor](xref:AutomationStation.Application.Services.AutomationProcessor) orchestrates evaluation, [ConditionEvaluator](xref:AutomationStation.Application.Services.ConditionEvaluator) compares event fields against conditions, and policies such as [CorrelationPolicy](xref:AutomationStation.Application.Policies.CorrelationPolicy) and `AutomationTriggerablePolicy` control how executions chain together.
- **Infrastructure** implements the ports: repositories over SQL Server, an in-memory action rate limiter, and HTTP clients for the Kanban and TOJ APIs.
- **Worker** is the runnable host. [Worker](xref:AutomationStation.Worker.Worker) is a .NET background service that consumes from RabbitMQ with manual acknowledgements (one message at a time) and feeds each event into the engine in a fresh DI scope.

## Guarantees

- **At-most-once handling** — processed message ids are persisted, so a redelivered event is not handled twice.
- **Per-action rate limiting** — actions are throttled in memory before they hit external APIs.
- **Auditable** — every execution (and its outcome) is written to history tables.

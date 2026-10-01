using AutomationStation.Application.Abstractions;

namespace AutomationStation.Application.Models;

public sealed record PlacementActionContext(
    Guid EntityId,
    Guid CorrelationId,
    Guid CausationEventId,
    Actor Actor,
    int CompanyId) : IActionContext;
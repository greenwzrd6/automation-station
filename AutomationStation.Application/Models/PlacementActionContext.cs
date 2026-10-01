using AutomationStation.Application.Abstractions;

namespace AutomationStation.Application.Models;

public sealed record PlacementActionContext(
    Guid EntityId,
    Guid CausationEventId,
    Actor Actor) : IActionContext;
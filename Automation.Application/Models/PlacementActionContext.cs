using Automation.Application.Abstractions;

namespace Automation.Application.Models;

public sealed record PlacementActionContext(
    Guid EntityId,
    string CausationEventId) : IActionContext;
namespace Automation.Application.Models;

public sealed record ActionContext(
    Guid EntityId,
    string CausationEventId);
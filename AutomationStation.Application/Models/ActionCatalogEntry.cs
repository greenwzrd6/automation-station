using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Application.Models;

public sealed record ActionCatalogEntry(
    string ActionType,
    TargetSystem TargetSystem,
    int PermittedActions,
    TimeSpan RateLimitWindow);
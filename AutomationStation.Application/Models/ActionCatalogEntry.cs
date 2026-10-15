using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Application.Models
{
    /// <summary>
    /// Record <c>ActionCatalogEntry</c> represents one action in the action catalog.
    /// </summary>
    /// <param name="ActionType">The type of action to perform, e.g., "CreatePlacement".</param>
    /// <param name="TargetSystem">The system the action should be executed in. See <see cref="Core.Automations.Systems.TargetSystem"/>.</param>
    /// <param name="PermittedActions">The number of times this action can be executed within the rate limit window.</param>
    /// <param name="RateLimitWindow">The time needed for the action to be executed again, in minutes.</param>
    public sealed record ActionCatalogEntry(
    string ActionType,
    TargetSystem TargetSystem,
    int PermittedActions,
    TimeSpan RateLimitWindow);
}
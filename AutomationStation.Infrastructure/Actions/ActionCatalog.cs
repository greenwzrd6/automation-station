using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Infrastructure.Actions;

public sealed class ActionCatalog : IActionCatalog
{
    private readonly IReadOnlyCollection<ActionCatalogEntry> _entries =
    [
        new ActionCatalogEntry(
            ActionType: "CreatePlacement",
            TargetSystem: TargetSystem.Kanban,
            PermittedActions: 100,
            RateLimitWindow: TimeSpan.FromMinutes(1))

        // Möjligt exempel för när TOJ är integrerat:
        //
        // new ActionCatalogEntry(
        //     ActionType: "CreateTask",
        //     TargetSystem: TargetSystem.TojSystem,
        //     PermittedActions: 50,
        //     RateLimitWindow: TimeSpan.FromMinutes(1))
    ];

    public bool TryGet(
        string actionType,
        TargetSystem targetSystem,
        out ActionCatalogEntry entry)
    {
        var matchingEntry = _entries.SingleOrDefault(
            candidate =>
                candidate.TargetSystem == targetSystem &&
                string.Equals(
                    candidate.ActionType,
                    actionType,
                    StringComparison.OrdinalIgnoreCase));

        if (matchingEntry is null)
        {
            entry = null!;
            return false;
        }

        entry = matchingEntry;
        return true;
    }
}
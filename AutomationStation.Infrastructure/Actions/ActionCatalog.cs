using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Infrastructure.Actions
{
    /// <summary>
    /// Class <c>ActionCatalog</c> represents all currently available actions.
    /// </summary>
    /// <remarks>Holds the hard-coded catalog entries that pair an action type with its target system and rate limit.
    /// The catalog is the source of truth for which actions the worker knows how to dispatch and for the per-action
    /// rate limits applied before execution.</remarks>
    public sealed class ActionCatalog : IActionCatalog
    {
        private readonly IReadOnlyCollection<ActionCatalogEntry> _entries =
        [
            new ActionCatalogEntry(
                ActionType: "CreatePlacement",
                TargetSystem: TargetSystem.Kanban,
                PermittedActions: 100,
                RateLimitWindow: TimeSpan.FromMinutes(1)),

             new ActionCatalogEntry(
                 ActionType: "CreateTask",
                 TargetSystem: TargetSystem.TojSystem,
                 PermittedActions: 10,
                 RateLimitWindow: TimeSpan.FromMinutes(1)),

            new ActionCatalogEntry(
                ActionType: "UpdateTaskStatus",
                TargetSystem: TargetSystem.TojSystem,
                PermittedActions: 100,
                RateLimitWindow: TimeSpan.FromMinutes(1))
        ];

        /// <summary>
        /// Looks up the catalog entry for an action type in a specific target system.
        /// </summary>
        /// <remarks>Action type matching ignores case. The target system must match exactly, so the same action type
        /// name can exist for more than one system.</remarks>
        /// <param name="actionType">The action type to find, for example "CreatePlacement".</param>
        /// <param name="targetSystem">The system the action is executed in.</param>
        /// <param name="entry">When found, the matching catalog entry; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if an entry exists for the action type and target system; otherwise
        /// <see langword="false"/>.</returns>
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
}
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Provides lookups into the catalog of actions that automations are allowed to perform.
    /// </summary>
    /// <remarks>The catalog is consulted before an action is dispatched: an action type that has no entry for the
    /// requested target system is rejected, and the matching <see cref="ActionCatalogEntry"/> supplies the permitted
    /// action count and rate limit window used to throttle execution.</remarks>
    public interface IActionCatalog
    {
        /// <summary>
        /// Attempts to find the catalog entry that governs the specified action type on the specified target system.
        /// </summary>
        /// <remarks>Action type matching is case-insensitive, while the target system must match exactly, so the same
        /// action type name can be registered for more than one system.</remarks>
        /// <param name="actionType">The action type to look up, for example "CreatePlacement".</param>
        /// <param name="targetSystem">The system the action would be executed in.</param>
        /// <param name="entry">When this method returns, contains the matching entry if one exists; otherwise
        /// <see langword="null"/>.</param>
        /// <returns><see langword="true"/> when an entry exists for the action type and target system; otherwise
        /// <see langword="false"/>.</returns>
        bool TryGet(
            string actionType,
            TargetSystem targetSystem,
            out ActionCatalogEntry entry);
    }
}

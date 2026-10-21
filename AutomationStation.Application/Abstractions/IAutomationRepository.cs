using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Persistence abstraction for loading automation definitions that should react to incoming events.
    /// </summary>
    /// <remarks>Implementations query the automation store; callers use this to find every enabled automation
    /// subscribed to an event type before evaluating its conditions and actions.</remarks>
    public interface IAutomationRepository
    {
        /// <summary>
        /// Retrieves all enabled automations that are subscribed to the given event type.
        /// </summary>
        /// <param name="eventType">The event type discriminator of the incoming integration event, matched against
        /// the automations' configured trigger event types.</param>
        /// <param name="cancellationToken">A token used to cancel the database query.</param>
        /// <returns>A task whose result is a read-only collection of enabled automations for the event type; empty
        /// when no automation is subscribed or all matching automations are disabled.</returns>
        Task<IReadOnlyCollection<Automation>>
            GetEnabledByEventTypeAsync(
                string eventType,
                CancellationToken cancellationToken);
    }
}
using AutomationStation.Application.Contracts;

namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Processes incoming integration events by evaluating them against enabled automations.
    /// </summary>
    /// <remarks>Implementations load the automations registered for the event type, check each
    /// automation's trigger conditions and execution policies (such as guards and rate limits), and dispatch
    /// the resulting actions for every automation that matches.</remarks>
    public interface IAutomationProcessor
    {
        /// <summary>
        /// Evaluates an integration event against all enabled automations and runs the actions of those that
        /// match.
        /// </summary>
        /// <param name="integrationEvent">The event to evaluate; its event type, source system, and payload
        /// determine which automations match.</param>
        /// <param name="cancellationToken">A token used to cancel processing while it is in flight.</param>
        /// <returns>A task that completes when every matching automation has been evaluated and its actions
        /// executed.</returns>
        Task ProcessAsync(
            IntegrationEvent integrationEvent,
            CancellationToken cancellationToken);
    }
}

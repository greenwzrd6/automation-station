using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Thens;

namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Dispatches automation actions to the handler responsible for performing them.
    /// </summary>
    /// <remarks>Implementations select the appropriate <see cref="IActionHandler"/> based on the action
    /// type and target system configured on the <see cref="Then"/>, so callers do not need to know which
    /// concrete executor performs a given action.</remarks>
    public interface IActionDispatcher
    {
        /// <summary>
        /// Executes the configured "then" action for the triggering event.
        /// </summary>
        /// <param name="then">The configured action to perform, including its type, target system, and parameters.</param>
        /// <param name="context">The action execution context, carrying the triggering integration event and the
        /// unique identifier of this execution.</param>
        /// <param name="cancellationToken">A token used to cancel the action while it is in flight.</param>
        /// <returns>A task that completes when the action has been executed.</returns>
        Task ExecuteAsync(
            Then then,
            ActionContext context,
            CancellationToken cancellationToken);
    }
}
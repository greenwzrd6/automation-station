using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Core.Automations.Thens;

namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Contract for executors that carry out the action described by a <see cref="Then"/> step of an automation.
    /// </summary>
    /// <remarks>Each implementation handles exactly one action type on one target system. Handlers are resolved by
    /// the action dispatcher using <see cref="ActionType"/> and <see cref="TargetSystem"/>, which must match the
    /// automation definition and the entries in the action catalog. Handlers validate that the triggering event and
    /// the action parameters contain everything they need, and report failures by throwing.</remarks>
    public interface IActionHandler
    {
        string ActionType { get; }

        TargetSystem TargetSystem { get; }

        /// <summary>
        /// Performs the action described by the given action definition using data from the triggering event.
        /// </summary>
        /// <param name="then">The action definition whose <c>Parameters</c> and <c>ExecutorId</c> configure the
        /// action to perform.</param>
        /// <param name="context">The triggering integration event and the identifier of this automation execution.</param>
        /// <param name="cancellationToken">A token used to cancel the action while it is in flight.</param>
        /// <returns>A task that completes when the action has been carried out.</returns>
        /// <exception cref="ArgumentException">The event payload or action parameters are missing values the action
        /// requires.</exception>
        /// <exception cref="InvalidOperationException">The action requires an executor identifier that is not set on
        /// the action definition.</exception>
        Task ExecuteAsync(
            Then then,
            ActionContext context,
            CancellationToken cancellationToken);
    }
}
using AutomationStation.Application.Contracts;

namespace AutomationStation.Application.Models
{
    /// <summary>
    /// Carries the information an action handler needs while executing one automation action.
    /// </summary>
    /// <remarks>Created by the automation processor once an automation has passed its trigger, guard, and rate-limit
    /// checks. The same context is passed to every action of the automation so that all actions of a single execution
    /// share one execution id and one triggering event.</remarks>
    /// <param name="Event">The integration event that triggered the automation run; its payload is the input the
    /// actions operate on.</param>
    /// <param name="ExecutionId">The id of the persisted automation execution record for this automation and event,
    /// used to correlate and de-duplicate the actions of the run.</param>
    public sealed record ActionContext(
        IntegrationEvent Event,
        Guid ExecutionId);
}

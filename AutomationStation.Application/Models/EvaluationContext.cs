using AutomationStation.Application.Contracts;
using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Models
{
    /// <summary>
    /// Record <c>EvaluationContext</c> bundles the automation being evaluated with the event that triggered it.
    /// </summary>
    /// <remarks>Handed to policy evaluation and action execution so handlers have both the automation definition and
    /// the triggering event in one place. The remaining properties project frequently used values from
    /// <see cref="Automation"/> and <see cref="Event"/> so callers do not have to reach through the record themselves.
    /// Value-based record equality applies, so two contexts over equal automation and event values are interchangeable.</remarks>
    /// <param name="Automation">The automation whose conditions and actions are being evaluated.</param>
    /// <param name="Event">The integration event that caused this evaluation.</param>
    public sealed record EvaluationContext(
        Automation Automation,
        IntegrationEvent Event)
    {
        public Guid AutomationId => Automation.Id;
        public string AutomationName => Automation.Name;
        public Guid CorrelationId => Event.CorrelationId;
        public Guid? CausationEventId => Event.CausationEventId;
        public Actor Actor => Event.Actor;
    }
}

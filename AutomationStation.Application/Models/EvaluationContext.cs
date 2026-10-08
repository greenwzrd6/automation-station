using AutomationStation.Application.Contracts;
using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Models
{
    public sealed record EvaluationContext(
        Automation Automation,
        IntegrationEvent Event)
    {
        public Guid AutomationId => Automation.Id;
        public string AutomationName => Automation.Name;
        public Guid CorrelationId => Event.CorrelationId;
        public Guid? CausationEventId => Event.CausationEventId;
        public string ActorId => Event.Actor.Id;
    }
}

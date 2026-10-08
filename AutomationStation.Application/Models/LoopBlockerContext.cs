using AutomationStation.Application.Contracts;
using AutomationStation.Core.Automations;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationStation.Application.Models
{
    public sealed record LoopBlockerContext(
        Automation Automation,
        IntegrationEvent Event)
    {
        public Guid AutomationId => Automation.Id;
        public Guid CorrelationId => Event.CorrelationId;
        public Guid? CausationEventId => Event.CausationEventId;
        public Actor Actor => Event.Actor;
    }
}

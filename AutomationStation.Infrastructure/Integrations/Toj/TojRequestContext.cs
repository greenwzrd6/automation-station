using AutomationStation.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationStation.Infrastructure.Integrations.Toj
{
    public sealed record TojRequestContext
    (
    Guid? CorrelationId,
    Guid? CausationEventId,
    Guid ExecutionId,
    Actor Actor);
}

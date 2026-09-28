using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationStation.Core.Events
{
    public sealed record EventMetadata(
    Guid EventId,
    Guid? CausationEventId,
    Guid? SourceAutomationId);
}

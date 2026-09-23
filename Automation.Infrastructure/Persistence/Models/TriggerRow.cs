using System;
using System.Collections.Generic;
using System.Text;

namespace Automation.Infrastructure.Persistence.Models
{
    internal sealed record TriggerRow(
        Guid AutomationId,
        string Name,
        bool Enabled,
        string EventType,
        string? SourceSystem);
}

using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Infrastructure.Persistence.Models
{
    internal sealed record ConditionRow(
        Guid AutomationId,
        string ConditionType,
        SourceSystem SourceSystem,
        string? ConfigurationJson);
}

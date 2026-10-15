using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Infrastructure.Models.Rows
{
    internal sealed record ConditionRow(
        Guid AutomationId,
        string ConditionType,
        SourceSystem SourceSystem,
        string? ConfigurationJson);
}

using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Infrastructure.Persistence.Models
{
    internal sealed record ActionRow(
        Guid AutomationId,
        string ActionType,
        TargetSystem? TargetSystem,
        string? ConfigurationJson,
        int ExecutionOrder);
}

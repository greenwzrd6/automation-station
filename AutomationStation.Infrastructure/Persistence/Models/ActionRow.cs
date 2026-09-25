namespace AutomationStation.Infrastructure.Persistence.Models
{
    internal sealed record ActionRow(
        Guid AutomationId,
        string ActionType,
        string? TargetSystem,
        string? ConfigurationJson,
        int ExecutionOrder);
}

namespace Automation.Infrastructure.Persistence.Models
{
    internal sealed record ConditionRow(
        Guid AutomationId,
        string ConditionType,
        string? SourceSystem,
        string? ConfigurationJson);
}

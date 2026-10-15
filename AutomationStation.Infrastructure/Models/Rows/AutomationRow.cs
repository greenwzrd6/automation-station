namespace AutomationStation.Infrastructure.Models.Rows
{
    internal sealed record AutomationRow(
        Guid Id,
        string Name,
        bool Enabled,
        bool AutomationTriggerable);
}
namespace Automation.Infrastructure.Persistence.Models
{
    internal sealed record AutomationRow(
        Guid Id,
        string Name,
        bool IsEnabled);
}
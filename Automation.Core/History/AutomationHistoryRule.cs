namespace Automation.Core.History
{
    public sealed record AutomationHistoryRule(
        Guid AutomationId,
        DateTime Timestamp);
}

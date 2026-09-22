namespace Automation.Core.Automations
{
    public sealed record When(
        string EventType,
        string EventSource,
        string Field,
        string Value);
}

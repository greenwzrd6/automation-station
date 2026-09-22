namespace Automation.Core.Automations
{
    public sealed record When(
        string EventType,
        string EventSource,
        IReadOnlyCollection<Condition> Conditions);
}

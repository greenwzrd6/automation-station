using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Core.Automations
{

    /// <summary>
    /// Record <c>When</c> represents when something should happen inside an automation.
    /// </summary>
    /// <param name="EventType">The type of event that triggers the <c>When</c></param>
    /// <param name="EventSource">The type of source the event came from <see cref="SourceSystem"/>.</param>
    /// <param name="Conditions">The conditions that must all match before the automation can execute. See <see cref="Condition"/>.</param>
    public sealed record When(
        string EventType,
        SourceSystem EventSource,
        IReadOnlyCollection<Condition> Conditions);
}

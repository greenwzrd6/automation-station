using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Core.Automations.Whens
{
    /// <summary>
    /// Record <c>Trigger</c> represents one half of a <see cref="When"/>.
    /// </summary>
    /// <param name="EventType">The type of event that causes an automation, e.g. "PlacementCreated".</param>
    /// <param name="SourceSystem">The system the event originated from.</param>
    public sealed record Trigger(
        string EventType, 
        SourceSystem SourceSystem);
}

using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Core.Automations.Whens
{
    /// <summary>
    /// Record <c>Trigger</c> represents one half of a <see cref="When"/>.
    /// </summary>
    /// <remarks>A trigger describes which events an automation listens for: the event type is matched
    /// case-insensitively and the source system must be equal to the event's source. An event that satisfies the
    /// trigger is then filtered further by the conditions of the same <see cref="When"/>; only when both match is
    /// the automation considered for execution.</remarks>
    /// <param name="EventType">The type of event that causes an automation, e.g. "PlacementCreated".</param>
    /// <param name="SourceSystem">The system the event originated from.</param>
    public sealed record Trigger(
        string EventType, 
        SourceSystem SourceSystem);
}

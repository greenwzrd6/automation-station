using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Infrastructure.Models.Rows
{
    /// <summary>
    /// Row model materialised from a database query over automation trigger definitions.
    /// </summary>
    /// <remarks>Used internally by the infrastructure persistence layer (for example with Dapper) to read
    /// which automations are triggered by a given event type and source system before the full automation is
    /// assembled.</remarks>
    /// <param name="AutomationId">Identifier of the automation the trigger belongs to.</param>
    /// <param name="EventType">The event type string the trigger listens for.</param>
    /// <param name="SourceSystem">The system that must produce the event for the trigger to fire.</param>
    internal sealed record TriggerRow(
        Guid AutomationId,
        string EventType,
        SourceSystem SourceSystem);
}

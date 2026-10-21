using System.Text.Json;
using System.Text.Json.Serialization;

using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Application.Contracts
{
    /// <summary>
    /// Record <c>IntegrationEvent</c> represents an event published by a source system and consumed by the automation
    /// worker.
    /// </summary>
    /// <remarks>Serialized to camelCase JSON (see the <see cref="JsonPropertyNameAttribute"/> on each parameter) and
    /// carried over RabbitMQ, so the same record round-trips between the publishing system and this worker. The event,
    /// correlation and causation identifiers together with the actor make the chain of events that led to an automation
    /// execution traceable; the payload holds the event-specific data and is not interpreted by the envelope itself.</remarks>
    /// <param name="EventId">The Id of the event, generated anew for each event.</param>
    /// <param name="EventType">The type of event, e.g. "PlacementCreated".</param>
    /// <param name="Source">The system the event originates from, see <see cref="SourceSystem"/>.</param>
    /// <param name="CompanyId">The company that the actor who triggered the event belongs to.</param>
    /// <param name="CorrelationId">Generated when an automation gets executed, if another automation gets triggered by that automation, it retains the same correlationId. </param>
    /// <param name="CausationEventId">The Id of the event that caused this event, if one did.</param>
    /// <param name="Actor">The person or automation who caused the event, see <see cref="Models.Actor"/>.</param>
    /// <param name="Payload">Json data that gets sent alongside the event. For "PlacementCreated" that would be entityId, columnId, etc.</param>
    public sealed record IntegrationEvent(

    [property: JsonPropertyName("eventId")]
    Guid EventId,

    [property: JsonPropertyName("eventType")]
    string EventType,

    [property: JsonPropertyName("source")]
    SourceSystem Source,

    [property: JsonPropertyName("companyId")]
    int CompanyId,

    [property: JsonPropertyName("correlationId")]
    Guid CorrelationId,

    [property: JsonPropertyName("causationEventId")]
    Guid? CausationEventId,

    [property: JsonPropertyName("actor")]
    Actor Actor,

    [property: JsonPropertyName("payload")]
    JsonElement Payload);
}

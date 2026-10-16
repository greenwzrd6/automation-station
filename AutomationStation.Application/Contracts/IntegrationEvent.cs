using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutomationStation.Application.Contracts
{
    /// <summary>
    /// Record<c>IntegrationEvent</c> represents the data 
    /// </summary>
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

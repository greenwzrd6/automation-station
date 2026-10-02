using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutomationStation.Application.Contracts;

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


using AutomationStation.Application.Models;
using System.Text.Json.Serialization;

namespace AutomationStation.Application.Contracts;

public sealed record IntegrationEvent<T>(

    [property: JsonPropertyName("eventId")]
    Guid EventId,

    [property: JsonPropertyName("eventType")]
    string EventType,

    [property: JsonPropertyName("source")]
    string Source,

    [property: JsonPropertyName("companyId")]
    Guid CompanyId,

    [property: JsonPropertyName("correlationId")]
    Guid CorrelationId,

    [property: JsonPropertyName("causationEventId")]
    Guid? CausationEventId,

    [property: JsonPropertyName("actor")]
    Actor Actor,

    [property: JsonPropertyName("payload")]
    T Payload);


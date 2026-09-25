using System.Text.Json.Serialization;

namespace AutomationStation.Application.Contracts;

public sealed record IntegrationEvent<T>(
    [property: JsonPropertyName("eventId")]
    string EventId,

    [property: JsonPropertyName("eventType")]
    string EventType,

    [property: JsonPropertyName("source")]
    string Source,

    [property: JsonPropertyName("payload")]
    T Payload);


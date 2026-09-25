using System.Text.Json.Serialization;

namespace AutomationStation.Application.Contracts;

public sealed record PlacementCreatedPayload(
    [property: JsonPropertyName("entityId")]
    Guid EntityId,

    [property: JsonPropertyName("columnId")]
    Guid ColumnId,

    [property: JsonPropertyName("sourceColumnId")]
    Guid? SourceColumnId);
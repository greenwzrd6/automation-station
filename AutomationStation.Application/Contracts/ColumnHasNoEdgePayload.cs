using System.Text.Json.Serialization;

namespace AutomationStation.Application.Contracts
{
    public sealed record ColumnHasNoEdgePayload(
        [property: JsonPropertyName("columnId")]
        Guid ColumnId);
}

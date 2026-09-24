using System.Text.Json.Serialization;

namespace Automation.Application.Contracts
{
    public sealed record ColumnHasNoEdgePayload(
        [property: JsonPropertyName("columnId")]
        Guid ColumnId);
}

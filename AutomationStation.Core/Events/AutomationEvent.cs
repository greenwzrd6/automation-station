using System.Text.Json;

namespace AutomationStation.Core.Events
{
    public sealed record AutomationEvent(
        Guid Id,
        string Type,
        string Source,
        JsonElement Payload);
}

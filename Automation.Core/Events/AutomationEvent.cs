using System.Text.Json;

namespace Automation.Core.Events
{
    public sealed record AutomationEvent(
        Guid Id,
        string Type,
        string Source,
        JsonElement Payload);
}

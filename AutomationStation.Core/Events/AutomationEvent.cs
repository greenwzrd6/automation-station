using AutomationStation.Core.Automations.Systems;
using System.Text.Json;

namespace AutomationStation.Core.Events
{
    public sealed record AutomationEvent(
        Guid Id,
        string Type,
        SourceSystem Source,
        JsonElement Payload);
}
